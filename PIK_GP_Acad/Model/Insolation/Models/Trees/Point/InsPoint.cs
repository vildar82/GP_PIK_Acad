using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.Runtime.Serialization;
using Catel.Services;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.UI;
using AcadLib;
using System.Xml.Serialization;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using AcadLib.XData;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчетная точка
    /// </summary>    
    public class InsPoint : InsPointBase, IInsPoint
    {
        [ExcludeFromSerialization]
        public VisualPointIllums VisualIllums { get; set; }

        public InsPoint () { }

        public InsPoint (InsModel model, Point3d pt) : base(pt, model)
        {               
            Window = WindowOptions.Default();
        }

        /// <summary>
        /// Создание расчетной точки из словарных записей объекта
        /// </summary>        
        public InsPoint (DBPoint dbPt, InsModel model) : base(dbPt, model)
        {            
        }       

        public bool IsVisualIllumsOn { get; set; }        

        /// <summary>
        /// Пока не используется
        /// ??? подумать над использованием !!! - обновление точки
        /// </summary>        
        public override void Initialize (TreeModel treeModel)
        {
            Model = treeModel.Model;
        }

        protected override void OnInitialized ()
        {
            base.OnInitialized();            
        }                  

        /// <summary>
        /// Расчет точки - зон освещенности и времени
        /// </summary>
        private void Calc ()
        {
            var building = Building;
            if (building == null)
            {
                Illums = null;
                InsValue = InsValue.Empty;
                return;
            }

            Illums = Model.CalcService.TreesCalc.CalcPoint(this);            
            InsValue = Model.CalcService.CalcTimeAndGetRate(Illums, building.BuildingType);
        }  

        /// <summary>
        /// Удаление - из расчета, отключение визуализации
        /// </summary>
        public override void Delete ()
        {
            Model.Tree.DeletePoint(this);
            VisualIllums.VisualIsOn = false;
            //VisualPoint.VisualIsOn = false; - удалится вместе с точкой на чертеже (т.к. это overrule точки)
            base.Delete();
        }

        private void OnIsVisualIllumsOnChanged ()
        {
            // Включение/выключение визуализации инсоляционных зон точки
            if (VisualIllums != null)
            {
                VisualIllums.VisualIsOn = IsVisualIllumsOn;
                SaveInsPoint();
            }
        }

        /// <summary>
        /// Описание точки
        /// </summary>        
        private string GetInfo()
        {
            var info = new StringBuilder();

            info.Append("Номер: ").Append(Number).Append(", Коорд. - ").Append(Point.ToStringEx()).Append(", Высота точки - ").Append(Height).AppendLine();
            info.Append("Инсоляция: ").Append(InsValue.Requirement.Name).Append(", Макс - ").
                Append(InsValue.MaxContinuosTimeString).Append(", Всего - ").Append(InsValue.TotalTimeString).AppendLine();
            var building = Building;
            if (building == null)
            {
                info.Append("Здание: Неопределено").AppendLine();
            }
            else
            {                
                info.Append("Здание: ").Append(building.BuildinTypeName).Append(", Высота - ").Append(building.Height).AppendLine();
            }
            info.Append("Окно: ширина - ").Append(Window.Width).Append(", Глубина четверти - ").Append(Window.Quarter).
                Append(", Конструкция - ").Append(Window.Construction.Name).Append(" ").Append(Window.Construction.Depth);

            return info.ToString();
        }

        /// <summary>
        /// Включение выключение зон визуализации
        /// </summary>
        /// <param name="onOff"></param>
        /// <param name="saveState"></param>
        public override void VisualOnOff(bool onOff, bool saveState)
        {
            // Изменение состояние на заданное                    
            if (saveState)
            {
                VisualIllums.VisualIsOn = onOff ? IsVisualIllumsOn : false;
            }
            else
            {
                IsVisualIllumsOn = onOff;
            }

            //VisualPoint.VisualIsOn = onOff;
        }

        /// <summary>
        /// Обноаление - расчета и визуализации
        /// </summary>
        public override void Update ()
        {
            try
            {
                Calc();
                UpdateVisual();                
            }
            catch (Exception ex)
            {
                InsService.ShowMessage(ex, "Ошибка обноаления точки.");
            }
        }

        public override void Clear ()
        {
            VisualIllums.VisualsDelete();            
            base.Clear();
        }

        public void SaveInsPoint ()
        {
            InsExtDataHelper.Save(this, Model.Doc);
        }

        /// <summary>
        /// Обноаление визуализации точки (зон инсоляции и описания точки)
        /// </summary>
        public void UpdateVisual ()
        {
            // Подготовка визуальных объектов
            // Визуализация зон инсоляции точки
            if (VisualIllums == null)
            {
                VisualIllums = new VisualPointIllums(this);
            }
            else
            {
                VisualIllums.VisualsDelete();
            }
            // Визуализация описания точки
            if (VisualPoint == null)
                VisualPoint = new VisualPoint(this);


            // Зоны освещ.
            if (IsVisualIllumsOn)
            {
                VisualIllums.VisualIsOn = true;                
            }
            // Описание точки
            //VisualPoint.VisualIsOn = true;


            Info = GetInfo();
        }

        /// <summary>
        /// Список значений для сохранения в словарь объекта чертежа
        /// </summary>
        /// <returns></returns>
        public override List<TypedValue> GetDataValues (Document doc)
        {
            List<TypedValue> values = new List<TypedValue>() {
                TypedValueExt.GetTvExtData( Height),                
                TypedValueExt.GetTvExtData(IsVisualIllumsOn)
            };
            return values;
        }

        public override void SetDataValues (List<TypedValue> values, Document doc)
        {
            if (values == null || values.Count != 2)
            {
                // Default 
                // Height = 0
                IsVisualIllumsOn = true;
            }
            else
            {
                Height = values[0].GetTvValue<int>();
                IsVisualIllumsOn = values[1].GetTvValue<bool>();
            }
        }

        public override DicED GetExtDic (Document doc)
        {
            DicED dicInsPt = new DicED("InsPoint");
            dicInsPt.AddRec("InsPointRec", GetDataValues(doc));
            dicInsPt.AddInner("Window", Window.GetExtDic(doc));
            return dicInsPt;
        }

        public override void SetExtDic (DicED dic, Document doc)
        {
            SetDataValues(dic.GetRec("InsPointRec")?.Values, doc);
            Window = new WindowOptions();
            Window.SetExtDic(dic.GetInner("Window"), doc);
        }
    }
}
