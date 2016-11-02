using System;
using System.Collections.Generic;
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

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Дом - состоит из блок-секций в ряд
    /// </summary>
    public class House : ModelBase
    {
        private Document doc = Application.DocumentManager.MdiActiveDocument;
        public House ()
        {            
        }   

        public FrontGroup FrontGroup { get; set; }
        
        /// <summary>
        /// Имя дома - в иделе = имени объекта принятого в проекте
        /// </summary>
        public string Name {
            get { return name; }
            set {
                name = value;
                SaveHouseNameToSection();
                RaisePropertyChanged();
            }
        }
        string name;

        /// <summary>
        /// Блок-секции дома
        /// </summary>
        public List<MapBuilding> Buildings { get; set; }

        public Polyline Contour { get { return contour; } set { DisposeContour(); contour = value;} }
        Polyline contour;

        public Error Error  { get { return error; } set { error = value; RaisePropertyChanged(); } }
        Error error;

        private void DisposeContour ()
        {
            if (Contour != null && !Contour.IsDisposed)
            {
                Contour.Dispose();
            }
        }

        /// <summary>
        /// Показать дом на карте
        /// </summary>
        public void Show()
        {
            try
            {                
                doc.Editor.Zoom(GetExtents());
            }
            catch { }
        }

        /// <summary>
        /// Определение домов в выбранной области
        /// </summary>        
        public static List<House> GetHouses (Scope scope, FrontGroup frontGroup)
        {
            var houses = new List<House>();
            // Определение домов из блок-секций
            var buildings = scope.Buildings;
            foreach (var building in buildings)
            {                
                if (!FindHouse(ref houses, building))
                {
                    var house = new House();
                    house.Buildings.Add(building);
                    houses.Add(house);
                }
            }
            // Для каждого дома - создание общей полилинии
            int countHouse = 1;
            foreach (var house in houses)
            {
                house.FrontGroup = frontGroup;
                house.DefineContour();
                // Заполнение оставшихся свойств дома
                house.DefineNameAuto(countHouse);
                countHouse++;
            }
            return houses;
        }

        /// <summary>
        /// Автоматическое определение имени дома
        /// </summary>
        /// <param name="countHouse"></param>
        private void DefineNameAuto (int countHouse)
        {
            if (!string.IsNullOrEmpty(Name)) return;
            if (Buildings != null)
            {
                Name = Buildings[0].Building.HouseName;
            }
            if (string.IsNullOrEmpty(Name))
            {
                Name = "Дом " + countHouse;
            }            
        }

        /// <summary>
        /// Сохранение имени дома в секции
        /// </summary>
        private void SaveHouseNameToSection ()
        {
            if (string.IsNullOrEmpty(Name)) return;
            if (Buildings!= null)
            {
                foreach (var item in Buildings)
                {
                    item.Building.HouseName = Name;
                    // TODO: Сохранить имя дома в расшир.данных объекта здания (IBuilding)                                   
                }
            }
        }

        /// <summary>
        /// Расчет фронтов дома
        /// </summary>
        public void CalcFront ()
        {
            if (Contour == null) return;

            var calcService = FrontGroup.Front.Model.CalcService;
            calcService.CalcFront.CalcHouse(this);
        }

        private void DefineContour ()
        {
            var pls = Buildings.Select(s => s.Contour).ToList();
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

        private static bool FindHouse (ref List<House> houses, MapBuilding building)
        {
            var offset = building.Contour.Offset(3, OffsetSide.Out).First();
            var findHouses = new List<House>();
            foreach (var house in houses)
            {
                foreach (var blInHouse in house.Buildings)
                {
                    var ptsIntersect = new Point3dCollection();
                    offset.IntersectWith(blInHouse.Contour, Intersect.OnBothOperands, ptsIntersect, IntPtr.Zero, IntPtr.Zero);
                    if (ptsIntersect.Count > 0)
                    {
                        findHouses.Add(house);
                        break;
                    }
                }
            }
            if (findHouses.Any())
            {
                if (findHouses.Skip(1).Any())
                {
                    // Объединение нескольких домов в один общий
                    var bls = findHouses.SelectMany(s => s.Buildings).ToList();
                    bls.Add(building);
                    var house = new House { Buildings = bls };
                    houses.Add(house);
                    foreach (var h in findHouses)
                    {
                        houses.Remove(h);
                    }
                }
                else
                {
                    findHouses[0].Buildings.Add(building);
                }
                return true;
            }
            return false;
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

        private Extents3d GetExtents ()
        {
            if (Contour != null)
            {
                return Contour.GeometricExtents;
            }
            if (Buildings != null)
            {
                var ext = new Extents3d();
                foreach (var item in Buildings)
                {
                    ext.AddExtents(item.ExtentsInModel);
                }
                return ext;
            }
            return new Extents3d();
        }
    }
}
