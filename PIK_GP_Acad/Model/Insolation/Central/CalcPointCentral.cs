using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Elements.Buildings;

namespace PIK_GP_Acad.Insolation.Central
{
    class CalcPointCentral
    {
        Point3d pt;
        IInsolationService insService;
        int maxHeight;
        CalcValuesCentral values;
        /// <summary>
        /// Начальный угол в плане (радиан). Начальное значение = 0 - восход.
        /// </summary>
        double angleStartOnPlane = 0;
        /// <summary>
        /// Конечный угол в плане (радиан). Начальное значение = 180 - заход
        /// </summary>
        double angleEndOnPlane = 180;

        public CalcPointCentral (Point3d pt, IInsolationService insService)
        {
            this.pt = pt;
            this.insService = insService;
            maxHeight = insService.Options.MaxHeight;
            values = insService.CalcValues;           
        }
        
        public List<IlluminationArea> Calc ()
        {
            var resAreas = new List<IlluminationArea>();
            // расчетный дом
            var buildingOwner = GetOwnerBuilding();
            // Корректировка расчетной точки
            pt = CorrectCalcPoint(buildingOwner);
            // Определение ограничений углов (начального и конечного) с учетом плоскости стены расчетного дома
            DefineAnglesStartEndByOwnerBuilding(buildingOwner);
            // дефолтные границы - от дефолтных стартовых углов (15, 165)
            var ext = GetCalcExtents();            

            var scope = insService.Map.GetScope(ext);            
            scope.Buildings.Remove(buildingOwner);
            

            var heights = scope.Buildings.GroupBy(g => g.Height);
            foreach (var bHeight in heights)
            {
                // зоны освещенности для домов этой высоты
                var illumsByHeight = CalcIllumsByHeight(bHeight.ToList(), bHeight.Key);
            }

            return resAreas;
        }        

        private List<IlluminationArea> CalcIllumsByHeight(List<InsBuilding> buildings, int height)
        {
            List<IlluminationArea> illums = new List<IlluminationArea>();

            double cShadow;
            double yShadow = values.YShadowLineByHeight(height, out cShadow);

            foreach (var build in buildings)
            {
                // Если дом полностью выше линии тени (сечения), то он полностью затеняет точку
                if (build.YMin > pt.Y)
                {
                    // Найти крайние точки дома для добавления зоны тени

                }
            }

            return illums;
        }

        /// <summary>
        /// Расчетная область - от точки
        /// </summary>        
        private Extents3d GetCalcExtents ()
        {
            // высота тени - отступ по земле от расчетной точки до линии движения тени
            double cSunPlane;
            double ySunPlane = values.YShadowLineByHeight(maxHeight, out cSunPlane);             
            // растояние до точки пересечения луча и линии тени
            double xRay = cSunPlane * Math.Tan(values.SunCalcAngleStart);
            Extents3d ext = new Extents3d(new Point3d (pt.X- xRay, pt.Y-ySunPlane,0),
                                          new Point3d (pt.X + xRay, pt.Y, 0));
            // Начальный угол в плане
            angleStartOnPlane = values.AngleSunOnPlane(ySunPlane, xRay);
            // Конечный угол в плане
            angleEndOnPlane = Math.PI - angleStartOnPlane;
            return ext;
        }

        private InsBuilding GetOwnerBuilding ()
        {
            var buildingOwner = insService.Map.GetBuildingInPoint(pt);
            if (buildingOwner== null)
            {
                throw new Exception($"Не определен дом принадлежащий расчетной точке.");
            }
            return buildingOwner;
        }

        private void DefineAnglesStartEndByOwnerBuilding (InsBuilding buildingOwner)
        {
            var illums = CalcIllumsByHeight(new List<InsBuilding> { buildingOwner }, buildingOwner.Height);
        }

        private Point3d CorrectCalcPoint (InsBuilding buildingOwner)
        {
            // перенос точки на 1 мм наружа от стороны фасада
            var plContour = buildingOwner.Contour;
            return Point3d.Origin;            
        }
    }
}
