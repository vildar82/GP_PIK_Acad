using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Errors;
using AcadLib.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation;
using PIK_GP_Acad.Insolation.Services;
using AcadLib.XData;
using PIK_DB_Projects;
using PIK_GP_Acad.Insolation.UI;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Дом - состоит из блок-секций
    /// Дом - соответствует Корпусу в проекте
    /// </summary>
    public class House : ModelBase, IDisposable
    {
        public House ()
        {

        }

        public House (FrontGroup frontGroup)
        {
            FrontGroup = frontGroup;
            Doc = FrontGroup.Front.Model.Doc;
            VisualFront = new VisualFront(Doc);
            //VisualFront.LayerVisual = FrontGroup.Front.Options.FrontLineLayer;            
        }   

        public Document Doc { get; set; }
        public FrontGroup FrontGroup { get; set; }        

        /// <summary>
        /// Имя дома - в иделе = имени объекта принятого в проекте
        /// </summary>
        public string Name {
            get { return name; }
            set {
                name = value;
                SaveHouseNameToSections();
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Центр дома
        /// </summary>        
        public Point3d GetCenter()
        {
            if (Contour == null || Contour.IsDisposed)
            {
                throw new Exception("Полилиния контура дома не определена.");
            }
            return Contour.Centroid();
        }

        string name;

        /// <summary>
        /// Высота расчета фронтов
        /// </summary>
        public double FrontHeight { get { return frontHeight; } set { frontHeight = value; RaisePropertyChanged(); } }
        double frontHeight;

        /// <summary>
        /// Блок-секции дома
        /// </summary>
        public ObservableCollection<MapBuilding> Sections { get { return sections; } set { sections = value; RaisePropertyChanged(); } }
        ObservableCollection<MapBuilding> sections = new ObservableCollection<MapBuilding>();

        public Polyline Contour { get { return contour; } set { DisposeContour(); contour = value;} }
        Polyline contour;

        public VisualFront VisualFront { get; set; }

        public List<FrontValue> FrontLines { get; set; }

        public List<List<FrontCalcPoint>> ContourSegmentsCalcPoints { get; set; }        

        public bool IsVisualFront {
            get { return isVisualFront; }
            set { isVisualFront = value; OnIsVisualFrontChanged(); RaisePropertyChanged(); }
        }
        bool isVisualFront;

        public Error Error  { get { return error; } set { error = value; RaisePropertyChanged(); Info = error?.Message; } }
        Error error;

        public string Info {
            get { return info; }
            set { info = value; RaisePropertyChanged(); }
        }
        string info;

        /// <summary>
        /// Идентификатор Корпуса в базе
        /// </summary>
        public int HouseId { get { return houseId; }
            set {
                if (houseId == value) return;
                houseId = value;
                SaveHouseIdToSections();
                SelectedHouseDb = FrontGroup.Front.FindHouseDb(value);
            }
        }
        int houseId;

        public HouseDbSel SelectedHouseDb {
            get { return selectedHouseDb; }
            set {
                if (selectedHouseDb == value) return;                
                var oldValue = selectedHouseDb;
                selectedHouseDb = value;
                RaisePropertyChanged();
                OnSelectedHouseDbChanged(oldValue);
            }
        }        
        HouseDbSel selectedHouseDb;

        /// <summary>
        /// Расчет фронтов дома
        /// </summary>
        public void Update ()
        {
            if (Contour == null) return;
            var calcService = FrontGroup.Front.Model.CalcService;
            try
            {
                // Отдельные линии инсоляции
                List<List<FrontCalcPoint>> contourSegmentsCalcPoints;
                FrontLines = calcService.CalcFront.CalcHouse(this, out contourSegmentsCalcPoints);
                ContourSegmentsCalcPoints = contourSegmentsCalcPoints;

                // Объединение линий фронтов для визуализациир
                var frontLinesCopy = FrontLines.Select(s => s.Clone()).ToList();
                var frontLinesMerged = FrontValue.Merge(ref frontLinesCopy);
                VisualFront.FrontLines = frontLinesMerged;
                VisualFront.VisualUpdate();
            }
            catch(Exception ex)
            {
                AddError(ex.ToString());
            }   
        }

        private void OnIsVisualFrontChanged ()
        {
            VisualFront.VisualIsOn = IsVisualFront;            
        }        

        private void OnSelectedHouseDbChanged(HouseDbSel oldValue)
        {
            if (object.ReferenceEquals(oldValue, SelectedHouseDb)) return;
            if (SelectedHouseDb != null)
            {
                HouseId = SelectedHouseDb.Id;
                SelectedHouseDb.Connect(this);
            }
            else
            {
                HouseId = 0;
            }
            if (oldValue!= null)
            {
                oldValue.Disconnect(this);
            }
            //FrontGroup.Front.DefineHouseDb();
        }

        /// <summary>
        /// Показать дом на карте
        /// </summary>
        public void Show()
        {
            try
            {
                using (Doc.LockDocument())
                {
                    Doc.Editor.Zoom(GetExtents());
                }                
            }
            catch { }
        }

        /// <summary>
        /// Определение имени дома - по блок-секциям или дефолтное по переданному индексу
        /// </summary>        
        public void DefineName(int countHouse)
        {
            if (!string.IsNullOrEmpty(Name)) return;
            // По идентификатору !!!???                    

            //TODO: FIX: определение имени дома по блок-секциям
            var houseNames = Sections.Where(w => !string.IsNullOrEmpty(w.Building.HouseName))
                .GroupBy(g => g.Building.HouseName).ToList();
            if (houseNames.Count == 1)
            {
                var houseName = houseNames.First().First();
                Name = houseName.Building.HouseName;
                HouseId = houseName.Building.HouseId;
            }

            if (string.IsNullOrEmpty(Name))
            {
                Name = "Дом " + countHouse;
            }
        }

        /// <summary>
        /// Сохранение имени дома в секции
        /// </summary>
        private void SaveHouseNameToSections()
        {
            //if (string.IsNullOrEmpty(Name)) return;            
            foreach (var item in Sections)
            {
                if (item.Building != null)
                {
                    item.Building.HouseName = Name;                    
                }
            }
        }

        private void SaveHouseIdToSections()
        {
            //if (string.IsNullOrEmpty(Name)) return;            
            foreach (var item in Sections)
            {
                if (item.Building != null)
                {
                    item.Building.HouseId = HouseId;
                }
            }
        }

        /// <summary>
        /// Добавление ошибки - относящейся к этому дому
        /// </summary>        
        public void AddError (string message)
        {
            if (Error == null)
            {
                var ext = GetExtents();
                Error = new Error("Ошибка.", ext, Matrix3d.Identity, System.Drawing.SystemIcons.Error);
            }
            Error.AdditionToMessage(message);
        }

        private Extents3d GetExtents()
        {
            if (Contour != null)
            {
                return Contour.GeometricExtents;
            }

            var ext = new Extents3d();
            foreach (var item in Sections)
            {
                ext.AddExtents(item.ExtentsInModel);
            }
            return ext;            
        }      

        /// <summary>
        /// Определение полилинии контура дома (объединением полилиний от блок-секций)
        /// </summary>
        public virtual void DefineContour ()
        {
            Contour?.Dispose();
            if (Sections.Count == 0) return;

            var pls = Sections.Select(s => s.Contour).ToList();

            // Предварительное соединение полилиний (близкие точки вершин разных полилиний - в среднюю вершину)
            // Сделал при определении домов

            if (pls.Count == 1)
            {
                Contour = (Polyline)pls.First().Clone();
            }
            else
            {
                using (var reg = pls.Union(null))
                {
                    var ptsRegByLoopType = reg.GetPoints2dByLoopType();
                    if (ptsRegByLoopType.Count == 1)
                    {
                        Contour = ptsRegByLoopType[0].Key.Cast<Point2d>().ToList().CreatePolyline();
                    }
                    else
                    {
                        // Объединение полилиний
                        var plsLoop = ptsRegByLoopType.Select(s => s.Key.Cast<Point2d>().ToList().CreatePolyline()).ToList();
                        try
                        {
                            var plMerged = plsLoop.Merge(2);
                            Contour = plMerged;
                        }
                        catch (Exception ex)
                        {
                            AddError(ex.Message);
                        }
                    }
                }
            }
#if TEST
            EntityHelper.AddEntityToCurrentSpace((Polyline)Contour.Clone());
#endif
        }

        /// <summary>
        /// Установить параметры дома из старого дома
        /// </summary>        
        public void SetDataFromOldHouse (House oldHouse)
        {
            FrontHeight = oldHouse.FrontHeight;
            IsVisualFront = oldHouse.IsVisualFront;
        }

        public void DisposeContour()
        {
            Contour?.Dispose();
        }        

        public void ClearVisual ()
        {
            VisualFront?.VisualsDelete();
        }

        public void UpdateVisual ()
        {
            VisualFront?.VisualUpdate();
        }

        public void Dispose ()
        {            
            VisualFront?.Dispose();
            if (FrontLines != null)
            {
                foreach (var item in FrontLines)
                {
                    if (item != null && item.Line != null && !item.Line.IsDisposed)
                    {
                        item.Line.Dispose();
                    }
                }
                FrontLines = null;
                if (VisualFront != null)
                {
                    VisualFront.FrontLines = null;
                }
            }
            DisposeContour();
        }
    }
}
