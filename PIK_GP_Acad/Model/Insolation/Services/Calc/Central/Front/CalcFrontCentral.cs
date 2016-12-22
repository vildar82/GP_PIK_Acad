using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Models;
using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Расчет фронтонов в центральном регионе
    /// </summary>
    public class CalcFrontCentral : ICalcFront
    {
        private CalcServiceCentral calcService;
        private ICalcTrees calcTrees;
        private House house;
        private FrontModel front;
        private FrontOptions frontOpt;
        private Map map;
        private InsModel model;
        private double delta;        

        public CalcFrontCentral (CalcServiceCentral calcService)
        {
            this.calcService = calcService;
            calcTrees = calcService.CalcTrees;
        }

        /// <summary>
        /// Расчет фронтонов дома
        /// </summary>        
        public List<FrontValue> CalcHouse (House house, out List<List<FrontCalcPoint>> contourSegmentsCalcPoints)
        {
            contourSegmentsCalcPoints = new List<List<FrontCalcPoint>>();
            if (house == null) return null;

            List<FrontValue> resFronts = new List<FrontValue>();

            this.house = house;
            front = house.FrontGroup?.Front;
            if (front == null) return resFronts;
            frontOpt = front.Options;
            model = front.Model;
            map = model.Map;
            delta = frontOpt.StepCalcPointInFront;

            var houseContour = house.Contour;
            // Расчет фронтов на каждом сегменте контура дома
            for (int i = 0; i < houseContour.NumberOfVertices; i++)
            {
                using (var seg = houseContour.GetLineSegment2dAt(i))
                {
                    List<FrontCalcPoint> segCalcPoints;
                    var segFronts = CalcSegment(seg, out segCalcPoints);
                    if (segFronts != null && segFronts.Any())
                    {
                        resFronts.AddRange(segFronts);
                    }
                    contourSegmentsCalcPoints.Add(segCalcPoints);
                }               
            }
            //resFronts = FrontValue.Merge(resFronts);
            return resFronts;
        }

        private List<FrontValue> CalcSegment (LineSegment2d seg, out List<FrontCalcPoint> calcPoints)
        {
            calcPoints = new List<FrontCalcPoint>();
            if (seg == null) return null;
            List<FrontValue> resFrontLines = new List<FrontValue>();            

            // Расчитанные точки сегмента
            calcPoints = GetFrontCalcPoints(seg, delta);
            // Определение фронтов
            var ptsIsCalced = calcPoints;//.Where(p => p.IsCalulated).ToList(); ???!!!
            var fPtPrew = ptsIsCalced.First();
            //fPtPrew.InsValue = ptsIsCalced.Skip(1).First().InsValue;
            var fPtStart = fPtPrew;
            for (int i =1; i< ptsIsCalced.Count; i++)
            {
                var item = ptsIsCalced[i];
                if (!item.IsCorner && fPtPrew.InsValue == InsRequirementEnum.None)
                {
                    fPtStart.InsValue = item.InsValue;
                }
                else if (fPtPrew.InsValue != item.InsValue && !item.IsCorner)
                {
                    // значение инс след точки
                    var iNext = i + 1;
                    if (iNext < ptsIsCalced.Count)
                    {
                        var itemNext = ptsIsCalced[iNext];
                        if (itemNext.IsCorner)
                        {
                            i++;
                            fPtPrew = itemNext;
                            continue;
                        }
                        else
                        {
                            if (itemNext.InsValue == fPtPrew.InsValue)
                            {
                                // Инс след точки совпадает с предыдущей - текущую игнорим.
                                continue;
                            }
                        }
                    }
                    // Создание фронта
                    var frontLine = CreateFrontLine(fPtStart, fPtPrew, seg);
                    if (frontLine != null)
                    {
                        resFrontLines.Add(frontLine);
                    }
                    fPtStart = item;
                }
                fPtPrew = item;
            }
            // Создание послежнего фронта
            var frontLineLast = CreateFrontLine(fPtStart, fPtPrew, seg);
            if (frontLineLast != null)
            {
                resFrontLines.Add(frontLineLast);
            }
            return resFrontLines;
        }

        private FrontValue CreateFrontLine (FrontCalcPoint fPtStart, FrontCalcPoint fPtEnd, LineSegment2d seg)
        {
            FrontValue frontLine = null;
            if (fPtStart.Point.IsEqualTo(fPtEnd.Point))
            {
                if (fPtStart.IsCorner && fPtEnd.IsCorner)
                {
                    return null;
                }
                // Участок фронта из 1 точки
                var pt1 = fPtStart.Point - seg.Direction * (delta * 0.5);
                var pt2 = fPtStart.Point + seg.Direction * (delta * 0.5);
                frontLine = new FrontValue(pt1, pt2, fPtStart.InsValue, frontOpt);
            }
            else
            {
                var ptEnd = fPtEnd.Point;
                var ptStart = fPtStart.Point;
                if (!fPtEnd.IsCorner)
                {
                    // Продлить линию на половину дельты
                    ptEnd = ptEnd + seg.Direction * (delta * 0.5);
                }
                if (!fPtStart.IsCorner)
                {
                    ptStart = ptStart - seg.Direction * (delta * 0.5);
                }

                frontLine = new FrontValue(ptStart, ptEnd, fPtStart.InsValue, frontOpt);
            }
            return frontLine;
        }

        /// <summary>
        /// Определение расчетных точек на сегменте
        /// </summary>        
        private List<FrontCalcPoint> GetFrontCalcPoints (LineSegment2d seg, double delta)
        {
            var calcPts =new List<FrontCalcPoint>();

            calcPts.Add(new FrontCalcPoint(seg.StartPoint, true));            

            // Добавление остальных точек с заданным шагом от стартовой до конечной
            var countSteps = Convert.ToInt32(seg.Length / delta)-1;
            if (countSteps == 0)
            {
                // Добавление средней точки сегмента
                calcPts.Add(new FrontCalcPoint(seg.MidPoint, false));
            }
            else
            {
                var ptPrew = seg.StartPoint;
                var vecDelta = seg.Direction * delta;
                for (int i = 0; i < countSteps; i++)
                {
                    var ptNext = ptPrew + vecDelta;
                    calcPts.Add(new FrontCalcPoint(ptNext, false));
                    ptPrew = ptNext;
                }
            }            

            calcPts.Add(new FrontCalcPoint(seg.EndPoint, true));

            // Определение блок-секций для точек
            DefineSectionInRange(ref calcPts);
            // Расчетные точки с определенными секциями - только их считать
            //calcPts = calcPts.Where(w => w.Section != null).ToList();

            // Расчет в точке
            var insPt = new InsPoint();
            insPt.Model = model;
            insPt.Window = house.FrontGroup.Options.Window;

            foreach (var calcFrontPt in calcPts)
            {
                if (calcFrontPt.Section == null) continue;
                if (calcFrontPt.IsCorner)
                {
                    calcFrontPt.IsCalulated = true;
                    continue;
                }

                insPt.Point = calcFrontPt.Point.Convert3d();
#if TEST
                //EntityHelper.AddEntityToCurrentSpace(new DBPoint(insPt.Point));
#endif
                insPt.Building = calcFrontPt.Section;
                // Уточнение высоты расчета фронта с учетом параметров заданных в здании (Section) - если не задана общая высота для фронта пользователем                
                insPt.Height = CalcHeightCalcPt(calcFrontPt, house.FrontHeight);                
                insPt.Building.InitContour();
                try
                {
                    var illums = calcTrees.CalcPoint(insPt);
                    var insValue = calcService.CalcTimeAndGetRate(illums, calcFrontPt.Section.BuildingType);
                    calcFrontPt.InsValue = insValue.Requirement.Type;
                    calcFrontPt.IsCalulated = true;
                }
                catch
                {
                    calcFrontPt.InsValue = InsRequirementEnum.None;
                    calcFrontPt.IsCorner = true;
                    // На угловых точках - может не рассчитаться пока
                    // Пропустить!?
                }
            }
            return calcPts;
        }

        /// <summary>
        /// Определение высоты расчетной точки
        /// </summary>
        /// <param name="calcFrontPt">Расчетная точка</param>
        /// <returns>Высота для расчета инсоляции в точке</returns>
        private double CalcHeightCalcPt(FrontCalcPoint calcFrontPt, double frontHeight)
        {
            double resHeightPt =0;
            var building = calcFrontPt.Section.Building;            
            if (frontHeight == 0)
            {
                // = Уровень здания + высота от пола до центра окна + высота первого этажа (если она задана, то это нежилой этаж)
                resHeightPt = InsPoint.DefaultHeightWindowCenter + building.HeightFirstFloor;
            }
            else
            {
                // Если задан уровень для всей группы фронтов, то прибавляем к нему только уровень секции
                resHeightPt = frontHeight;
            }
            return resHeightPt;
        }

        /// <summary>
        /// Определение расчетной секции для точек
        /// </summary>        
        private void DefineSectionInRange (ref List<FrontCalcPoint> calcPts)
        {   
            foreach (var item in calcPts)
            {
                item.DefineSection(map);
            }
        }       
    }    
}