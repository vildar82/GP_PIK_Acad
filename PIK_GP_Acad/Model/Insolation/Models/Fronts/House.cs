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
using MicroMvvm;
using System.Windows.Media;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Дом - состоит из блок-секций
    /// Дом - соответствует Корпусу в проекте
    /// </summary>
    public class House : ModelBase, IDisposable, IEquatable<House>
    {
        private bool isInitialized;
        private bool isDefaultName;
        private int numberHouseInGroup;        

        //public House (FrontGroup frontGroup)
        //{
        //    FrontGroup = frontGroup;
        //    Doc = FrontGroup.Front.Model.Doc;
        //    VisualFront = new VisualFront(Doc);            
        //}

        public House(InsModel model, int index)
        {
            Model = model;
            Doc = model.Doc;
            Index = index;
            VisualFront = new VisualFront(Doc);
        }
                
        public InsModel Model { get; set; }
        public Document Doc { get; set; }
        public FrontGroup FrontGroup { get; set; }  

        /// <summary>
        /// Настройки дома - окна для расчета фронтов
        /// </summary>
        public HouseOptions Options { get { return options; } set { options = value; RaisePropertyChanged(); SaveHouseOptions(); } }
        HouseOptions options;
        
        public bool IsOverrideOptions { get { return isOverrideOptions; } set { isOverrideOptions = value; RaisePropertyChanged(); } }
        bool isOverrideOptions;

        /// <summary>
        /// Уникальный индекс (номер) дома - на карте
        /// </summary>
        public int Index { get; set; }      

        /// <summary>
        /// Имя дома - в иделе = имени объекта принятого в проекте
        /// </summary>
        public string Name {
            get { return name; }
            set {
                if (name != value)
                {
                    name = value;
                    SaveHouseNameToSections();
                    RaisePropertyChanged();
                }
            }
        }
        string name;        

        /// <summary>
        /// Высота расчета фронтов
        /// </summary>
        public int FrontLevel {
            get { return frontLevel; }
            set {
                if (frontLevel != value)
                {
                    frontLevel = value; RaisePropertyChanged(); SaveFrontLevelToSections();
                    if (isInitialized)
                        Update();
                }
            }
        }      
        int frontLevel;

        /// <summary>
        /// Блок-секции дома
        /// </summary>
        public ObservableCollection<MapBuilding> Sections { get { return sections; } set { sections = value; RaisePropertyChanged(); } }
        ObservableCollection<MapBuilding> sections = new ObservableCollection<MapBuilding>();

        /// <summary>
        /// Единый контур. Для кольцевого дома - внешний контур
        /// </summary>
        public Polyline Contour { get { return contour; } set { DisposeContour(); contour = value;} }
        Polyline contour;
        ///// <summary>
        ///// Только для кольцевого дома - внутренний контур
        ///// </summary>
        //public Polyline ContourInner { get { return contourInner; } set { DisposeContour(); contourInner = value; } }
        //Polyline contourInner;        

        public VisualFront VisualFront { get; set; }
        //public List<FrontValue> FrontLines { get; set; }
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
        /// Выбранный для связывания с базой объект
        /// </summary>
        public HouseDbSel SelectedHouseDb {
            get { return selectedHouseDb; }
            set {
                if (value != null && !value.Equals(selectedHouseDb))
                {
                    var oldValue = selectedHouseDb;
                    selectedHouseDb = value;                    
                    OnSelectedHouseDbChanged(oldValue);
                }
                RaisePropertyChanged();
            }
        }        
        HouseDbSel selectedHouseDb;

        /// <summary>
        /// Расчет фронтов дома
        /// </summary>
        public void Update (int? numberHouseInGroup = null)
        {
            //DisposeFrontLines();
            if (Contour == null)
                return;
            if (numberHouseInGroup.HasValue)
                this.numberHouseInGroup = numberHouseInGroup.Value;
            DefineName();

            // Визуализация зданий в доме
            UpdateVisual();

            var calcService = FrontGroup.Front.Model.CalcService;
            try
            {
                // Отдельные линии инсоляции
                List<List<FrontCalcPoint>> contourSegmentsCalcPoints;
                var frontLines = calcService.CalcFront.CalcHouse(this, out contourSegmentsCalcPoints);
                ContourSegmentsCalcPoints = contourSegmentsCalcPoints;

                // Объединение линий фронтов для визуализациир                
                //var frontLinesCopy = FrontLines.Select(s => s.Clone()).ToList();
                var frontLinesMerged = FrontValue.Merge(ref frontLines);
                VisualFront.FrontLines = frontLinesMerged;                
                IsVisualFront = FrontGroup.IsVisualFrontOn;
            }
            catch (UserBreakException)
            {
                throw;
            }
            catch (Exception ex)
            {
                AddError(ex.ToString());
            }   
        }

        /// <summary>
        /// Определение связывания дома с объектом базы - по сохраненным в секциях идентификаторам
        /// </summary>
        public void DefineConnectionDbSel()
        {
            // Если во всех секциях сохранен один идентификатор
            var houseIds = Sections.Where(w => w.Building.HouseId != 0).GroupBy(g => g.Building.HouseId).ToList();
            if (houseIds.Count ==1)
            {
                // Поиск объекта для связывания
                var houseDbSel = Model.FindHouseDb(houseIds.First().Key);
                if (houseDbSel != null)
                {
                    houseDbSel.Connect(this);
                    SelectedHouseDb = houseDbSel;                  
                }
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

        private void OnIsVisualFrontChanged ()
        {
            VisualFront.VisualIsOn = IsVisualFront;            
        }        

        private void OnSelectedHouseDbChanged(HouseDbSel oldValue)
        {
            if (oldValue?.Equals(SelectedHouseDb) == true) return;
            if (SelectedHouseDb != null)
            {
                //HouseId = SelectedHouseDb.Id;
                SelectedHouseDb.Connect(this);
                // Сохранение связанного объекта в блок-секциях
                SaveHouseIdToSections();
            }
            //else
            //{
            //    HouseId = 0;
            //}
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
        private void DefineName()
        {
            isInitialized = false;
            var houseNames = Sections.Where(w => !string.IsNullOrEmpty(w.Building.HouseName))
                .GroupBy(g => g.Building.HouseName).ToList();
            if (houseNames.Count == 1)
            {
                isDefaultName = false;
                var houseName = houseNames.First().First();
                Name = houseName.Building.HouseName;
                //HouseId = houseName.Building.HouseId;                
            }
            else
            {
                isDefaultName = true;
                Name = "Дом " + numberHouseInGroup;
            }

            // Определение уровня расчета фронтов
            var levels = Sections.Where(w => w.Building.FrontLevel != 0).GroupBy(g => g.Building.FrontLevel).ToList();
            if (levels.Count ==1)
            {
                FrontLevel = levels.First().Key;
            }

            // определение настроек
            var options = Sections.Where(w => w.Building.HouseOptions != null).GroupBy(g => g.Building.HouseOptions).ToList();
            if (options.Count == 1)
            {
                Options = options.First().Key;
            }

            //// Определение связанного объекта - если во всех секциях сохранен один идентификатор
            //DefineConnectionDbSel(); // Уже определено в InsModel

            isInitialized = true;
        }        

        /// <summary>
        /// Сохранение имени дома в секции
        /// </summary>
        private void SaveHouseNameToSections()
        {
            if (isDefaultName || !isInitialized)
                return;
            //if (string.IsNullOrEmpty(Name)) return;            
            using (Doc.LockDocument())
            using (var t = Doc.TransactionManager.StartTransaction())
            {
                foreach (var item in Sections)
                {
                    if (item.Building != null && item.Building.HouseName != Name)
                    {
                        item.Building.HouseName = Name;                        
                        item.Building.SaveDboDict();
                    }
                }
                t.Commit();
            }
        }       

        private void SaveHouseIdToSections()
        {
            //if (string.IsNullOrEmpty(Name)) return;  
            if (!isInitialized || Sections.All (s=>s.Building.HouseId == SelectedHouseDb.Id))
                return;
            using (Doc.LockDocument())
            using (var t = Doc.TransactionManager.StartTransaction())
            {
                foreach (var item in Sections)
                {
                    if (item.Building != null && item.Building.HouseId != SelectedHouseDb.Id)
                    {
                        item.Building.HouseId = SelectedHouseDb.Id;
                        item.Building.SaveDboDict();
                    }
                }
                t.Commit();
            }
        }

        private void SaveFrontLevelToSections()
        {
            if (!isInitialized || Sections == null || Sections.All(s=>s.Building.FrontLevel == FrontLevel))
                return;            
                        
            using (Doc.LockDocument())
            using (var t = Doc.TransactionManager.StartTransaction())
            {
                foreach (var item in Sections)
                {
                    if (item.Building != null && item.Building.FrontLevel != FrontLevel)
                    {
                        item.Building.FrontLevel = FrontLevel;
                        item.Building.SaveDboDict();
                    }
                }
                t.Commit();
            }
        }

        private void SaveHouseOptions()
        {
            if (Options == null)
            {
                IsOverrideOptions = false;
            }
            else
            {
                IsOverrideOptions = true;
            }
            if (!isInitialized || Sections.All(s => s.Building.HouseOptions == Options))
                return;
            using (Doc.LockDocument())
            using (var t = Doc.TransactionManager.StartTransaction())
            {
                foreach (var item in Sections)
                {
                    if (item.Building != null && item.Building.HouseOptions != Options)
                    {
                        item.Building.HouseOptions = Options;
                        item.Building.SaveDboDict();
                    }
                }
                t.Commit();
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

        /// <summary>
        /// Получение уровня расчета фронтов для этого дома
        /// </summary>
        /// <returns></returns>
        public int GetCalcFrontLevel()
        {
            return FrontLevel == 0 ? FrontGroup.FrontLevel : FrontLevel;
        }

        public WindowOptions GetCalcFrontWindow()
        {
            return Options?.Window ?? FrontGroup.Options.Window;
        }

        public Extents3d GetExtents()
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
                foreach (var item in pls)
                {
                    item.Wedding(TreeModel.TolerancePoints);
#if DEBUG
                    //EntityHelper.AddEntityToCurrentSpace((Entity)item?.Clone());
#endif
                }

                using (var reg = pls.Union(null))
                {
#if DEBUG
                    //EntityHelper.AddEntityToCurrentSpace((Region)reg?.Clone());            
#endif
                    var ptsRegByLoopType = reg.GetPoints2dByLoopType();
                    if (ptsRegByLoopType.Count == 1)
                    {
                        Contour = ptsRegByLoopType[0].Key.Cast<Point2d>().ToList().CreatePolyline();
                    }
                    else
                    {                        
                        // Если это "кольцевой" дом (замкнутый контур из блок-секций)
                        if (ptsRegByLoopType.Count == 2 && ptsRegByLoopType.Any(p => p.Value == BrepLoopType.LoopInterior))
                        {
                            // Будет один дом состоящий из двух полилиний - внешней и внутренней                            
                            // Внешний контур
                            Contour = ptsRegByLoopType.First(p=>p.Value== BrepLoopType.LoopExterior).Key.Cast<Point2d>().ToList().CreatePolyline();
                            // Внутренний контур
                            //ContourInner = ptsRegByLoopType.First(p => p.Value == BrepLoopType.LoopInterior).Key.Cast<Point2d>().ToList().CreatePolyline();
                        }
                        else
                        {
                            // Объединение полилиний                            
                            var plsLoop = ptsRegByLoopType.Select(s => s.Key.Cast<Point2d>().ToList().CreatePolyline()).ToList();
                            try
                            {
#if DEBUG
                                //EntityHelper.AddEntityToCurrentSpace(plsLoop);
#endif
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
            }            
            // Прополка полилилинии
            Contour?.Wedding(TreeModel.TolerancePoints);
#if TEST
            EntityHelper.AddEntityToCurrentSpace((Polyline)Contour?.Clone());
            TestDrawContourVertexText();
            //EntityHelper.AddEntityToCurrentSpace((Polyline)ContourInner?.Clone());
#endif
        }  

        public void ClearVisual ()
        {
            if (Sections != null)
                foreach (var item in Sections)
                {
                    item.DisposeVisual();
                }
            VisualFront?.VisualsDelete();
        }

        public void UpdateVisual ()
        {
            // отрисовка зданий (с дополнениями по фронту)
            if (Sections != null)
                foreach (var item in Sections)
                {
                    item.UpdateVisual();
                    item.Visual.IsVisualizedInFront = true;
                }            
            // Отрисовка фронтов
            VisualFront?.VisualUpdate();            
        }

        public void DisposeContour()
        {
            Contour?.Dispose();            
        }

        public void DisposeVisuals()
        {
            VisualFront?.Dispose();            
            FrontGroup = null;
            //DisposeFrontLines();
        }

        //private void DisposeFrontLines ()
        //{
            //if (FrontLines != null)
            //{
            //    foreach (var item in FrontLines)
            //    {
            //        item?.Dispose();
            //    }
            //    FrontLines = null;
            //}
        //}

        public void Dispose ()
        {
            Contour?.Dispose();            
            DisposeVisuals();
        }

        private void TestDrawContourVertexText()
        {
            if (Contour == null || Contour.IsDisposed) return;
            for (int i = 0; i < Contour.NumberOfVertices; i++)
            {
                var vertex = Contour.GetPoint2dAt(i);
                var text = new DBText();
                text.TextString = i.ToString();
                text.Position = vertex.Convert3d();
                text.Height = 2;
                EntityHelper.AddEntityToCurrentSpace(text);
            }
        }

        public bool Equals(House other)
        {
            return Index == other?.Index;
        }
        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }
    }
}