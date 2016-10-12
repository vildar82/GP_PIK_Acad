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

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчетная точка
    /// </summary>
    [Serializable]
    public class InsPoint : InsPointBase, IInsPoint
    {
        public const string DataRec = "InsPoint";        

        public bool IsVisualIllumsOn { get; set; }
        [ExcludeFromSerialization]
        private VisualPointIllums VisualIllums { get; set; }        

        public InsPoint () { }

        public InsPoint (InsModel model, Point3d pt) : base(pt, model)
        {               
            Window = WindowOptions.Default();
        }

        /// <summary>
        /// Создание расчетной точки из словарных записей объекта
        /// </summary>        
        public InsPoint (List<TypedValue> values, DBPoint dbPt, InsModel model) : base(dbPt, model)
        {
            SetDataValues(values);            
        }

        public override void Initialize (TreeModel treeModel)
        {
            Model = treeModel.Model;
        }

        protected override void OnInitialized ()
        {
            base.OnInitialized();
            EditPoint = new TaskCommand(OnEditPointExecute, OnEditPointCanExecute);
            DeletePoint = new TaskCommand(OnDeletePointExecute);
        }        

        public TaskCommand EditPoint { get; private set; }
        public TaskCommand DeletePoint { get; private set; }

        public override string DataRecName { get { return DataRec; } }

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

        private async Task OnEditPointExecute ()
        {
            var building = Building;
            if (building == null) return;
            var oldBuildingType = building.BuildingType;

            var insPointVM = new InsPointViewModel(this);
            var uiVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            if (await uiVisualizerService.ShowDialogAsync(insPointVM) == true)
            {
                // Обновление расчета точки
                Update();

                if (oldBuildingType != building.BuildingType)
                {
                    // Учет изменения типа здания для всех точек на этом здании
                    var insReq = Model.CalcService.DefineInsRequirement(InsValue.MaxContinuosTime, InsValue.TotalTime, building.BuildingType);
                    var pointsInBuilding = Model.Tree.GetPointsInBuilding(building);
                    foreach (var item in pointsInBuilding)
                    {
                        if (item != this)
                        {
                            item.InsValue.Requirement = insReq;
                            item.UpdateVisual();
                        }
                    }
                }

                // Обновление елочек
                Model.Tree.UpdateVisualTree(this);

                // Сохранение точки в словарь
                InsExtDataHelper.Save(this, Model.Doc);
            }
        }
        private bool OnEditPointCanExecute ()
        {
            return Building != null;
        }

        private async Task OnDeletePointExecute ()
        {
            Delete();
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

            VisualPoint.VisualIsOn = onOff;
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

        /// <summary>
        /// Обноаление визуализации точки (зон инсоляции и описания точки)
        /// </summary>
        private void UpdateVisual ()
        {
            // Подготовка визуальных объектов
            // Визуализация зон инсоляции точки
            if (VisualIllums == null)
                VisualIllums = new VisualPointIllums(this);
            // Визуализация описания точки
            if (VisualPoint == null)
                VisualPoint = new VisualPoint(this);


            // Зоны освещ.
            if (IsVisualIllumsOn)
                VisualIllums.VisualUpdate();
            // Описание точки
            VisualPoint.VisualIsOn = true;


            Info = GetInfo();
        }

        /// <summary>
        /// Список значений для сохранения в словарь объекта чертежа
        /// </summary>
        /// <returns></returns>
        public override List<TypedValue> GetDataValues ()
        {
            List<TypedValue> values = new List<TypedValue>() {
                new TypedValue((int)DxfCode.ExtendedDataInteger32, Height),
                new TypedValue((int)DxfCode.ExtendedDataReal, Window.Width),
                new TypedValue((int)DxfCode.ExtendedDataReal, Window.Quarter),
                new TypedValue((int)DxfCode.ExtendedDataInteger16, Window.IsCustomAngle? 1:0),
                new TypedValue((int)DxfCode.ExtendedDataReal, Window.ShadowAngle),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, Window.Construction.Name),
                new TypedValue((int)DxfCode.ExtendedDataReal, Window.Construction.Depth),
            };
            return values;
        }

        private void SetDataValues (List<TypedValue> values)
        {
            if (values.Count == 7)
            {
                int index = 0;
                Height = GetTVValue<int>(values[index++]);
                Window = new WindowOptions(
                        GetTVValue<double>(values[index++]),
                        GetTVValue<double>(values[index++]),
                        GetTVValue<int>(values[index++]) == 0 ? false : true,
                        GetTVValue<double>(values[index++]),
                        WindowConstruction.Find(GetTVValue<string>(values[index++])));
            }
        }

        private T GetTVValue<T> (TypedValue tv)
        {
            T res;
            if (tv.Value is T)
            {
                res = (T)tv.Value;
            }
            else
            {
                res = default(T);
            }
            return res;
        }
    }
}
