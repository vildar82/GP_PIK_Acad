using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Insolation;
using PIK_GP_Acad.Insolation.Options;

namespace PIK_GP_Acad.Model.Insolation.ShadowMap
{
    public class ShadowCentral : IShadowService
    {
        private Dictionary<string, Vector2d> dictVecShadows = new Dictionary<string, Vector2d>();

        private Map map;
        private InsOptions options;
        /// <summary>
        /// Кол шагов от начального угла до конечного
        /// </summary>
        private int CountSteps;
        private int OneStepTime;        
        /// <summary>
        /// Широта в радианах 
        /// </summary>
        private double fi;

        public ShadowCentral (Map map)
        {
            //this.options = map.Options;
            this.map = map;
            CountSteps = 150; // если 0 это восход, а 180 - закат. То расчетные углы от 15 до 165 = 150
            OneStepTime = options.ShadowDegreeStep * 4; // 4 минуты в 1 градусе
            // Широта в радианах
            fi = ((double)options.Region.Latitude).ToRadians();
        }

        /// <summary>
        /// Расчет тени от здания
        /// </summary>        
        public Shadow Calc (IBuilding building)
        {
            //Shadow shadow = new Shadow(building);
            //using (var t = map.db.TransactionManager.StartOpenCloseTransaction())
            //{
            //    var buildingContour = building.GetContourInModel();
            //    // От начального расчетного угла (часа) - до последнего
            //    for (int i = 15; i < 165; i++)
            //    {
            //        // Вектор тени от точки на заданной высоте и угла луча
            //        var vecShadow = GetShadowVector(building.Height, i);
            //        // Вершины контура здания
            //        List<Point2d> buildingVertex = GetContourVertexes(buildingContour);
            //        // Определение граничных вершин здания (определяющих тень)
            //        List<ShadowBoundaryPoint> shadowBoundaryPts = GetBoundaryPoints(buildingVertex, vecShadow);

            //    }                
            //    t.Commit();
            //}
            //return shadow;
            return null;
        }

        private List<Point2d> GetContourVertexes (Polyline contour)
        {
            List<Point2d> vertexes = new List<Point2d>();
            for (int i = 0; i < contour.NumberOfVertices; i++)
            {
                var pt = contour.GetPoint2dAt(i);
                vertexes.Add(pt);
            }
            return vertexes;
        }

        private Vector2d GetShadowVector (int height, int rayDegree)
        {
            Vector2d resVec;
            string key = height + "_" + rayDegree;
            if (!dictVecShadows.TryGetValue(key, out resVec))
            {   
                // высота тени - основание тени (отступ по земле от расчетной точки до линии движения тени)
                double hSunPlane = height * Math.Tan(fi);
                // гипотенуза плоскости солнца
                double cSunPlane = height / Math.Cos(fi);
                double hRay = cSunPlane * Math.Tan(((double)rayDegree).ToRadians());
                resVec = new Vector2d(hRay, hSunPlane);
                dictVecShadows.Add(key, resVec);
            }
            return resVec;
        }

        private List<ShadowBoundaryPoint> GetBoundaryPoints (List<Point2d> buildingVertexes, Vector2d vecShadow)
        {
            List<ShadowBoundaryPoint> boundaryPts = new List<ShadowBoundaryPoint>();
            // крайние точки тени - с мин и макс расстоянием на перпендикулярном векторе к лучу
            var vecFront = vecShadow.GetPerpendicularVector();
            var vecFrontNormal = vecFront.GetNormal();

            var firstPt = buildingVertexes[0];
            Ray2d rayShadow = new Ray2d(firstPt, vecShadow);            
            Vector2d vec1ToRay = new Vector2d ();
            foreach (var buildVertex in buildingVertexes)
            {
                var bPt = new ShadowBoundaryPoint(buildVertex);
                var ptInRay = rayShadow.GetNormalPoint(buildVertex).Point;
                var vecToRay = ptInRay - buildVertex;
                if (vec1ToRay.Length == 0)                
                    vec1ToRay = vecToRay;                
                else                
                    bPt.LenToRay = vecToRay.Length * (vecToRay.IsCodirectionalTo(vec1ToRay)? 1:-1);                
                boundaryPts.Add(bPt);
            }
            var ptsOrderByLen = boundaryPts.OrderBy(p => p.LenToRay);

            var minBPt = ptsOrderByLen.First();
            var maxBPt = ptsOrderByLen.Last();
            var indexMinBPt = boundaryPts.IndexOf(minBPt);
            var indexMaxBPt = boundaryPts.IndexOf(maxBPt);
            if (indexMinBPt > indexMaxBPt)
            {
                var temp = indexMinBPt;
                indexMinBPt = indexMaxBPt;
                indexMaxBPt = temp;
            }
            var vecFromMinBptToMax = maxBPt.Point - minBPt.Point;
            // Определение внутренних и наружных точек здания по отношению к границе тени от этого здания (внешние и внутренние точки)
            var firstBptFromMinToMax = boundaryPts[indexMinBPt + 1];
            var angleToRay = vecShadow.GetAngleTo(firstBptFromMinToMax.Point - minBPt.Point);
            var typeBPtFromMinToMax = angleToRay <= 90d.ToRadians() ? BoundaryPointType.Internal : BoundaryPointType.External;
            for (int i = 0; i <= indexMinBPt; i++)
            {
                boundaryPts[i].Type = typeBPtFromMinToMax;
            }
            var typeOtherBPt = typeBPtFromMinToMax == BoundaryPointType.External ? BoundaryPointType.Internal : BoundaryPointType.External;
            boundaryPts.Where(b => b.Type == BoundaryPointType.None).Iterate(b => b.Type = typeOtherBPt);

            return boundaryPts;
        }
    }

    class ShadowBoundaryPoint
    {
        public Point2d Point { get; set; }
        public double LenToRay { get; set; }
        public BoundaryPointType Type { get; set; }

        public ShadowBoundaryPoint(Point2d pt)
        {
            Point = pt;
        }
    }

    enum BoundaryPointType
    {
        None,
        /// <summary>
        /// Внешняя точка дома - на границе тени
        /// </summary>
        External,
        /// <summary>
        /// Внутренняя точка дома - внутри границы тени
        /// </summary>
        Internal        
    }
}
