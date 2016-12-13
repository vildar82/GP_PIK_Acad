using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;
using Autodesk.AutoCAD.DatabaseServices;
using NetLib;
using Autodesk.AutoCAD.Geometry;
using AcadLib.Geometry;
using AcadLib.Errors;

namespace PIK_GP_Acad.Insolation.Services.Export
{
    /// <summary>
    /// Дом - трансформация
    /// </summary>
    public class HouseTransform //: IDisposable
    {
        private House house;
        //private Tolerance toleranceCell = new Tolerance(0.01,0.1);

        public HouseTransform(House house)
        {
            this.house = house;
            Id = house.HouseId;         
        }

        public List<InsCell> Cells { get; set; }
        public int Id { get; set; }

        /// <summary>
        /// Нормализация дома - приведение к ортогональному виду (минимальный поворот до ортогональности вокруг точки центра дома) 
        /// </summary>
        public void Normalize()
        {
            // определение ячеек инсоляции по 3,6м
            Cells = GetCells();
            // Поворот до ортогональности
            ToOrtho();
        }

        /// <summary>
        /// Трансформация дома
        /// </summary>        
        public void Trans(Matrix2d matTrans)
        {            
            foreach (var item in Cells)
            {
                item.PtCenter = item.PtCenter.TransformBy(matTrans);
            }
        }

        /// <summary>
        /// Поворот до ортогональности
        /// </summary>
        private void ToOrtho()
        {
            var pt1 = Cells.First();
            var pt2 = Cells.Skip(1).First();
            var vecSeg = pt2.PtCenter - pt1.PtCenter;
            double angleToOrtho;
            if (!vecSeg.Angle.IsOrthoAngle(out angleToOrtho))
            {
                // Поворот дома на угол angleToOrtho до ортогонального положения вокруг центра контура
                var ptCenter = house.GetCenter();
                var matRotate = Matrix2d.Rotation(angleToOrtho, ptCenter.Convert2d());
                Trans(matRotate);
            }
        }       

        /// <summary>
        /// Определение ячеек инсоляции
        /// </summary>        
        private List<InsCell> GetCells()
        {
            var resCells = new List<InsCell>();            
            // Перебор сегментов - определение ячеек модулей инсоляции для каждого сегмента
            var points = house.ContourSegmentsCalcPoints;
            foreach (var segPoints in points)
            {    
                var segCells = new List<InsCell>();
                var startPt = segPoints.First().Point;
                var vecModule = (segPoints.Last().Point - startPt).GetNormal() *InsCell.ModuleSize;
                var vecHalfModule = vecModule * 0.5;
                var vecInsidePlHalfModule = GetVectorHalfModuleInsideContour(vecHalfModule, segPoints.Skip(1).First().Point);

                var calcPtsInModule = new List<FrontCalcPoint>();
                calcPtsInModule.Add(segPoints.First());
                FrontCalcPoint lastPt = null;
                bool isFirstCell = true;
                // Перебор расчетных точек сегмента - определение ячеек модулей инсоляции
                for (int i =1; i < segPoints.Count; i++)
                {
                    var calcPt = segPoints[i];
                    lastPt = calcPt;
#if DEBUG
                    var lenSeg = (segPoints.Last().Point - startPt).Length;
#endif
                    var vec = calcPt.Point - startPt;                    
                    if (vec.Length>InsCell.ModuleSize)// 0.1 - запас
                    {
                        if (vec.Length - InsCell.ModuleSize < 0.1)
                        {
                            calcPtsInModule.Add(calcPt);
                        }
                        var cellPt = GetCenterModulePoint(startPt, vecHalfModule, vecInsidePlHalfModule);
                        var cell = new InsCell(cellPt, calcPtsInModule, vecModule);
                        // Если это первый угловой модуль - то объединение с последним модулем предыдущего сегмента
                        if (isFirstCell && resCells.Any())
                        {
                            isFirstCell = false;
                            var lastModuleInLastSeg = resCells.Last();
                            try
                            {
                                if (OverlayCells(lastModuleInLastSeg, cell))
                                {
                                    startPt = startPt + vecModule;                                    
                                    calcPtsInModule = new List<FrontCalcPoint>();
                                    calcPtsInModule.Add(lastPt);
                                    lastPt = null;
                                    continue;
                                }                                
                            }
                            catch (Exception ex)
                            {
                                Inspector.AddError(ex.Message, System.Drawing.SystemIcons.Error);
                                startPt = startPt + vecModule;                                
                                calcPtsInModule = new List<FrontCalcPoint>();
                                calcPtsInModule.Add(lastPt);
                                lastPt = null;
                                continue;
                            }                            
                        }
                        segCells.Add(cell);
                        startPt = startPt + vecModule; 
                        calcPtsInModule = new List<FrontCalcPoint>();
                        calcPtsInModule.Add(lastPt);
                        lastPt = null;                            
                    }
                    else
                    {
                        calcPtsInModule.Add(calcPt);
                    }
                }
                // Проверка последней точки - если остаток больше половины модуля - то добавляем целый модуль
                if (lastPt != null && (lastPt.Point- startPt).Length > (InsCell.ModuleSize * 0.5))
                {
                    var cellPt = GetCenterModulePoint(startPt, vecHalfModule, vecInsidePlHalfModule);
                    var cell = new InsCell(cellPt, calcPtsInModule, vecModule);
                    segCells.Add(cell);
                }                
                resCells.AddRange(segCells);
            }

            // Первая угловая ячекйа - наложение с последней ячейкой
            try
            {
                var lastCell = resCells.Last();
                if (OverlayCells(resCells.First(), lastCell))
                {
                    resCells.Remove(lastCell);
                }
            }
            catch(Exception ex)            
            {
                Inspector.AddError(ex.Message, System.Drawing.SystemIcons.Error);
            }
#if TEST
            // Подпись ячеек            
            foreach (var item in resCells)
            {
                item.TestDraw();
            }
#endif
            return resCells;
        }

        /// <summary>
        /// Наложение совпадающих ячеек (в углах)
        /// </summary>        
        /// <returns>True - ячейки наложились (для cell1 уточнено значение инсоляции). False - ячейки расположены сонаправлено друг за другом.
        /// Exception - ячейки неправильно расположены</returns>
        private bool OverlayCells (InsCell cell1, InsCell cell2)
        {
            if (cell1.Overlay(cell2))
            {                
                return true;
            }
            else
            {
                // Если сегменты не сонаправленны друг за другом - то ошибка расположения модулей
                if (!cell1.Direction.IsParallelTo(cell2.Direction, InsCell.Tolerance) &&
                    cell1.PtCenter.IsEqualTo(cell2.PtCenter))
                {
                    throw new Exception ($"Ошибка расположения ячеек - '{cell1}' и '{cell2}'");                                
                }
            }
            return false;
        }

        /// <summary>
        /// Определение номеров строк и столбцов ячеек инсоляции
        /// </summary>
        public void DefineNumCells()
        {
            foreach (var cell in Cells)
            {
                cell.DefineNumCell();
            }

            // Проверка наложения ячеек - не должно быть ячеек с одинаковым номером строки и столбца
            var dublicateCells = Cells.GroupBy(g => new { row = g.Row, col = g.Column }).Where(w=>w.Skip(1).Any());
            foreach (var dubl in dublicateCells)
            {
                // Ошибка
                var fp = dubl.First();
                var err = $"Наложение ячеек инсоляции! Координата {fp.PtCenterOrig}. Ячейка в Excel [{dubl.Key.row};{dubl.Key.col}]";
                var ext = fp.PtCenterOrig.GetRectangleFromCenter(InsCell.ModuleSize).Convert3d();
                Inspector.AddError(err, ext, Matrix3d.Identity, System.Drawing.SystemIcons.Error);
            }

#if TEST
            // Подпись ячеек            
            foreach (var item in Cells)
            {
                item.TestDraw();
            }
#endif
        }

        /// <summary>
        /// Точка центра модуля
        /// </summary>
        /// <param name="startModulePt">Стартовая расчетная точка в модуле</param>
        /// <param name="vecHalfModule">Вектор половины модуля (по направлению сегмента)</param>
        /// <param name="vecHalfModuleInsidePl">Вектор половины модуля внутрь полилинии</param>
        /// <returns>Точка центра модуля</returns>
        private Point2d GetCenterModulePoint(Point2d startModulePt, Vector2d vecHalfModule, Vector2d vecHalfModuleInsidePl)
        {            
            return startModulePt + vecHalfModule + vecHalfModuleInsidePl;            
        }

        /// <summary>
        /// Вектор половины длины модуля внутрь контура
        /// </summary>
        /// <param name="vecHalfModule">Вектор половины длины модуля</param>
        /// <param name="ptOnSegment">Точка на сегменте</param>
        /// <returns>Вектор половины длины модуля направленный внутрь контура</returns>
        private Vector2d GetVectorHalfModuleInsideContour(Vector2d vecHalfModule, Point2d ptOnSegment)
        {
            // Перпендикуляр внутрь полилинии
            var vecModuleHalfPerp = vecHalfModule.GetPerpendicularVector();
            var ptPerp = ptOnSegment + vecModuleHalfPerp;
            var contour = house.Contour;
            if (contour.IsPointInsidePolygon(new Point3d(ptPerp.X, ptPerp.Y, contour.Elevation)))
            {
                return vecModuleHalfPerp;
            }
            else
            {
                return vecModuleHalfPerp.Negate();
            }
        }

        public void Dispose()
        {
            //contour?.Dispose();
        }
    }
}
