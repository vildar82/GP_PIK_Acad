using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;
using Autodesk.AutoCAD.DatabaseServices;
using NetLib;

namespace PIK_GP_Acad.Insolation.Services.Export
{
    /// <summary>
    /// Дом - трансформация
    /// </summary>
    public class HouseTransform : IDisposable
    {
        private House house;
        private List<InsCell> cells;
        private Polyline contour;
        private List<List<FrontCalcPoint>> points;

        public HouseTransform(House house)
        {
            this.house = house;
            this.contour = house.Contour;
            this.points = house.ContourSegmentsCalcPoints.Select(s=>s.Select(p=>(FrontCalcPoint)p.Clone()).ToList()).ToList();
        }
        
        /// <summary>
        /// Нормализация дома - приведение к ортогональному виду (минимальный поворот до ортогональности вокруг точки центра дома) 
        /// </summary>
        public void Normalize()
        {
            // Поворот до ортогональности
            ToOrtho();
        }

        /// <summary>
        /// Поворот до ортогональности
        /// </summary>
        private void ToOrtho()
        {
            var seg = contour.GetLineSegment2dAt(0);
            double angleToOrtho;
            if (!seg.Direction.Angle.IsOrthoAngle(out angleToOrtho))
            {
                // Поворот дома на угол angleToOrtho до ортогонального положения
                var ptCenter = house.GetCenter();                
            }
        }

        public void Dispose()
        {
            contour?.Dispose();
        }
    }
}
