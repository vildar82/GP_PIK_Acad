using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.UI;
using AcadLib;
using System.Xml.Serialization;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using AcadLib.XData;
using MicroMvvm;
using AcadLib.Statistic;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчетная точка
    /// </summary>    
    public class InsPoint : InsPointBase, IInsPoint
    {
        /// <summary>
        /// Дефолтная высота до центра окуна от пола
        /// </summary>
        public const double DefaultHeightWindowCenter = 1.8;

        public VisualPointIllums VisualIllums { get; set; }

        public InsPoint () { }        

        public InsPoint (InsModel model, Point3d pt) : base(pt, model)                   
        {               
            Window = WindowOptions.Default;
            Height = DefaultHeightWindowCenter; // Высота центра окна от пола
            ReportPoint = new RelayCommand(OnReportPointExecute);
        }

        /// <summary>
        /// Создание расчетной точки из словарных записей объекта
        /// </summary>        
        public InsPoint(DBPoint dbPt, InsModel model) : base(dbPt, model)
        {
            ReportPoint = new RelayCommand(OnReportPointExecute);
        }

        public RelayCommand ReportPoint { get; set; }

        public bool IsVisualIllumsOn { get { return isVisualIllumsOn; }
            set { isVisualIllumsOn = value; RaisePropertyChanged(); OnIsVisualIllumsOnChanged(); } }
        bool isVisualIllumsOn;

        /// <summary>
        /// Пока не используется
        /// ??? подумать над использованием !!! - обновление точки
        /// </summary>        
        public override void Initialize (TreeModel treeModel)
        {
            Model = treeModel.Model;
        }              

        /// <summary>
        /// Расчет точки - зон освещенности и времени
        /// </summary>
        private void Calc ()
        {
            var building = Building;
            bool withOwnerBuilding = true;            
            if (building == null)
            {
                //Illums = null;
                //InsValue = InsValue.Empty;
                //return;
                withOwnerBuilding = false;                
            }            
            try
            {
                building?.InitContour();
                Illums = Model.CalcService.CalcTrees.CalcPoint(this, withOwnerBuilding);
                InsValue = Model.CalcService.CalcTimeAndGetRate(Illums, building?.BuildingType ?? BuildingTypeEnum.Living);
            }
            catch(UserBreakException)
            {
                throw;
            }
            catch
            {
                building?.Contour?.Dispose();                
                Illums = null;
                InsValue = InsValue.Empty;
            }            
        }  

        /// <summary>
        /// Удаление - из расчета, отключение визуализации
        /// </summary>
        public override void Delete ()
        {
            Model.Tree.DeletePoint(this);
            if (VisualIllums != null)
                VisualIllums.VisualIsOn = false;            
            base.Delete();
        }

        /// <summary>
        /// Отчет по точке - вставка таблицы отчета
        /// </summary>
        public void InsPointReport()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            using (doc.LockDocument())
            {
                var report = new InsPointReport(this, doc.Database);
                report.CalcRows();
                var table = report.Create();
                report.Insert(table, doc);
                PluginStatisticsHelper.AddStatistic();
            }
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

        private void OnReportPointExecute()
        {
            InsPointReport();
        }

        /// <summary>
        /// Описание точки
        /// </summary>        
        private string GetInfo()
        {
            var info = new StringBuilder();

            info.Append("Номер: ").Append(Number).Append(", Коорд. - ").Append(Point.ToStringEx()).Append(", Высота точки - ").Append(Height).Append("м.").AppendLine();
            info.Append("Инсоляция: ").Append(InsValue.Requirement.Name).Append(", Макс - ").
                Append(InsValue.MaxContinuosTimeString).Append(", Всего - ").Append(InsValue.TotalTimeString).AppendLine();
            var building = Building;
            if (building == null)
            {
                info.Append("Здание: Неопределено").AppendLine();
            }
            else
            {                
                info.Append("Здание: ").Append(building.BuildinTypeName).Append(", Высота - ").Append(building.Building.Height).AppendLine();
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
            Calc();
            UpdateVisual();
        }

        public override void ClearVisual ()
        {
            VisualIllums?.VisualsDelete();            
            //base.Clear();
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
            // Зоны освещ.            
            VisualIllums.VisualIsOn = IsVisualIllumsOn;

            // Визуализация описания точки
            if (VisualPoint == null)
                VisualPoint = new VisualPoint(this);            

            Info = GetInfo();
        }

        /// <summary>
        /// Список значений для сохранения в словарь объекта чертежа
        /// </summary>
        /// <returns></returns>
        public override List<TypedValue> GetDataValues (Document doc)
        {
            var tvk = new TypedValueExtKit();
            tvk.Add("Height", Height);
            tvk.Add("IsVisualIllumsOn", IsVisualIllumsOn);
            return tvk.Values;            
        }

        public override void SetDataValues (List<TypedValue> values, Document doc)
        {
            var dictValues = values?.ToDictionary();
            Height = dictValues.GetValue("Height", 0);
            IsVisualIllumsOn = dictValues.GetValue("IsVisualIllumsOn", true);            
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
            SetDataValues(dic?.GetRec("InsPointRec")?.Values, doc);
            Window = new WindowOptions();
            Window.SetExtDic(dic?.GetInner("Window"), doc);
        }

        public override void Dispose ()
        {
            VisualIllums?.Dispose();
        }
    }
}
