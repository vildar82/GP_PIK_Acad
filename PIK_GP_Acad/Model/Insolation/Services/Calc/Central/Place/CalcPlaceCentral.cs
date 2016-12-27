using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Models;
using AcadLib;
using AcadLib.Geometry;

namespace PIK_GP_Acad.Insolation.Services
{
    public class CalcPlaceCentral : ICalcPlace
    {
        private Dictionary<double, TileLevel> dictLevels;
        private CalcServiceCentral calcService;
        private ICalcTrees calcTrees;
        private Polyline pl;
        private PlaceOptions placeOptions;
        private List<TileLevel> levels;
        private InsModel model;
        private double step;
        private double stepHalf;
        private double stepQuart;

        public CalcPlaceCentral(CalcServiceCentral calcService)
        {
            this.calcService = calcService;
            calcTrees = calcService.CalcTrees;            
        }

        /// <summary>
        /// Расчет площадки
        /// </summary>        
        public List<Tile> CalcPlace (Place place)
        {
            List<Tile> tiles;
            dictLevels = new Dictionary<double, TileLevel>();
            placeOptions = place.PlaceModel.Options;
            levels = placeOptions.Levels.OrderByDescending (o=>o.TotalTimeMin).ToList();
            model = place.PlaceModel.Model;            
            step = placeOptions.TileSize;
            stepHalf = step * 0.5;
            stepQuart = step * 0.25;
            using (pl = place.PlaceId.Open(OpenMode.ForRead) as Polyline)
            {
                place.Area = pl.Area.Round(2);                    
                // Нарезка площадки на ячейки (tiles)
                tiles = DividePlace();
                //  расчет каждой ячейке (точка - без оуна и без здания)
                var insPt = new InsPoint();
                insPt.Model = model;
                insPt.Window = null;
                insPt.Building = null;
                insPt.Height = 0;
                foreach (var tile in tiles)
                {
                    insPt.Point = tile.Point.Convert3d();
                    try
                    {
                        var illums = calcTrees.CalcPoint(insPt, false);
                        tile.InsValue = calcService.CalcTimeAndGetRate(illums, Elements.Buildings.BuildingTypeEnum.Living);
                        tile.Level = DefineLevel(tile.InsValue.TotalTime);
                    }
                    catch (UserBreakException)
                    {
                        throw;
                    }
                    catch
                    {
                        tile.InsValue = InsValue.Empty;
                        tile.Level = TileLevel.Empty;
                        // На угловых точках - может не рассчитаться пока
                        // Пропустить!?
                    }                    
                }
            }
            return tiles;
        }        

        /// <summary>
        /// Разделение площадки на ячейки
        /// </summary>        
        private List<Tile> DividePlace ()
        {
            List<Tile> resTiles = new List<Tile>();
            // Построение вертик и гор линий по границе полилинии с заданным шагом
            var ext = pl.GeometricExtents;
            var horTiles = Divide(ext.MinPoint.X, ext.MaxPoint.X);
            var verticTiles = Divide(ext.MinPoint.Y, ext.MaxPoint.Y);
            var plane = new Plane();            
            foreach (var horT in horTiles)
            {
                // Построение вертик линии
                using (var verticLine = new Line(new Point3d(horT, ext.MinPoint.Y, 0), new Point3d(horT, ext.MaxPoint.Y, 0)))
                {
                    // пересечение вертик линии c контуром
                    var ptsIntersect = new Point3dCollection();
                    verticLine.IntersectWith(pl, Intersect.OnBothOperands,plane, ptsIntersect, IntPtr.Zero, IntPtr.Zero);
                    if (ptsIntersect.Count == 0) continue;
                    if (ptsIntersect.Count==1)
                    {
                        //var pt = new Point2d(horT,ptsIntersect[0].Y);
                        //resTiles.Add(new Tile(pt));
                    }
                    else
                    {
                        var pts = ptsIntersect.Cast<Point3d>().OrderBy(o => o.Y);                       
                        var ptIntersectLower = pts.First();                        
                        foreach (var ptIntersectTop in pts.Skip(1))
                        {
                            var ptMid = ptIntersectLower.Center(ptIntersectTop);                            
                            if (pl.IsPointInsidePolygon(ptMid))
                            {
                                foreach (var vertT in verticTiles)
                                {
                                    if (vertT >= ptIntersectTop.Y)
                                    {
                                        break;
                                    }
                                    else if(vertT - ptIntersectLower.Y >= stepQuart)
                                    {                                        
                                        var pt = new Point2d(horT, vertT);
                                        resTiles.Add(new Tile(pt, step, step));
                                    }                                                                     
                                }
                            }
                            ptIntersectLower = ptIntersectTop;
                        }
                    }
                }
            }
            return resTiles;
        }

        /// <summary>
        /// Разделить границу площадки на горизонтальные ячейки. Включая начальные координаты
        /// </summary>        
        private List<double> Divide (double startCoord, double endCoord)
        {
            var resTiles = new List<double>();

            //resTiles.Add(startCoord);
            var count = (endCoord - startCoord) / step;
            double curCoord = startCoord - stepHalf;
            for (int i = 0; i < count; i++)
            {
                curCoord += step;
                resTiles.Add(curCoord);
            }
            var lastStepFactor = count - (int)count;
            if (lastStepFactor > 0.5)
            {
                curCoord += step;
                resTiles.Add(curCoord);
            }

            //resTiles.Add(endCoord);

            return resTiles;
        }

        /// <summary>
        /// Определение уровня освещенности ячейки
        /// </summary>        
        private TileLevel DefineLevel (double timeMin)
        {
            TileLevel level;
            if (!dictLevels.TryGetValue(timeMin, out level))
            {
                level = levels.FirstOrDefault(l => timeMin >= l.TotalTimeMin);
                if (level == null)
                {
                    level = TileLevel.Empty;
                }
                dictLevels.Add(timeMin, level);
            }
            return level;
        }
    }
}
