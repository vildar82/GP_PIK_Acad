using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Models;

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
        public List<FrontValue> CalcHouse (House house)
        {
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
                    var segFronts = CalcSegment(seg);
                    if (segFronts != null && segFronts.Any())
                    {
                        resFronts.AddRange(segFronts);
                    }
                }               
            }
            resFronts = FrontValue.Merge(resFronts);
            return resFronts;
        }

        private List<FrontValue> CalcSegment (LineSegment2d seg)
        {
            if (seg == null) return null;
            List<FrontValue> resFrontLines = new List<FrontValue>();            

            // Расчитанные точки сегмента
            var calcPoints = GetFrontCalcPoints(seg, delta);
            // Определение фронтов
            var ptsIsCalced = calcPoints.Where(p => p.IsCalulated);
            FrontCalcPoint fPtPrew = ptsIsCalced.First();
            fPtPrew.InsValue = ptsIsCalced.Skip(1).First().InsValue;
            FrontCalcPoint fPtStart = fPtPrew;
            foreach (var item in ptsIsCalced.Skip(1))
            {
                if (fPtPrew.InsValue != item.InsValue || item.IsCorner)
                {
                    if (item.IsCorner)
                    {
                        fPtPrew = item;
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
            DefineSectionInRange(ref calcPts, 0, calcPts.Count-1);
            // Расчетные точки с определенными секциями - только их считать
            //calcPts = calcPts.Where(w => w.Section != null).ToList();

            // Расчет в точке - без окна!
            var insPt = new InsPoint();
            insPt.Model = model;            
            insPt.Window = null;

            foreach (var calcFrontPt in calcPts)
            {
                if (calcFrontPt.Section == null) continue;
                if (calcFrontPt.IsCorner)
                {
                    calcFrontPt.IsCalulated = true;
                    continue;
                }

                insPt.Point = calcFrontPt.Point.Convert3d();
                insPt.Building = calcFrontPt.Section;                                
                try
                {
                    var illums = calcTrees.CalcPoint(insPt);
                    var insValue = calcService.CalcTimeAndGetRate(illums, calcFrontPt.Section.BuildingType);
                    calcFrontPt.InsValue = insValue.Requirement.Type;
                    calcFrontPt.IsCalulated = true;
                }
                catch
                {
                    // На угловых точках - может не рассчитаться пока
                    // Пропустить!?
                }
            }
            return calcPts;
        }

        /// <summary>
        /// Определение расчетной секции для точек
        /// </summary>        
        private void DefineSectionInRange (ref List<FrontCalcPoint> calcPts, int startIndex, int endIndex)
        {            
            if (endIndex <= startIndex) return;
            
            // Секция стартовой точки
            if (!DefineSectionInPoint(ref calcPts,ref startIndex, 1))
            {
                // Не определена секция 
                return;
            }            
            // Секция конечной точки
            if (!DefineSectionInPoint(ref calcPts,ref endIndex, -1))
            {
                return;
            }
            
            if (startIndex< endIndex)
            {
                // Если у стартовой точки и у конечной одна и таже секция, то все промежуточные точки с тойжке секцией
                if (calcPts[startIndex].Section == calcPts[endIndex].Section)
                {
                    var sec = calcPts[startIndex].Section;
                    for (int i = startIndex+1; i < endIndex; i++)
                    {
                        calcPts[i].Section = sec;
                    }
                }
                else
                {
                    // Деление диапазона пополам и в нем определение секций
                    var centerIndex = (endIndex - startIndex) / 2;
                    DefineSectionInRange(ref calcPts, startIndex, centerIndex);
                    DefineSectionInRange(ref calcPts, centerIndex+1, endIndex);
                }
            }            
        }

        private bool DefineSectionInPoint (ref List<FrontCalcPoint> calcPts,ref int index, int dir)
        {
            bool res = false;
            if (index >= calcPts.Count || index <0) return res;

            var calcPt = calcPts[index];
            if (calcPt.DefineSection(map))
            {
                res = true;
            }
            else
            {
                // Сдвиг на след точку
                index += dir;
                res = DefineSectionInPoint(ref calcPts, ref index, dir);
            }            
            return res;
        }
    }

    class FrontCalcPoint
    {
        public FrontCalcPoint (Point2d pt, bool isCorner)
        {
            Point = pt;
            IsCorner = isCorner;
        }

        public bool IsCorner { get; set; }
        public Point2d Point { get; set; }
        public MapBuilding Section { get; set; }
        /// <summary>
        /// Игнорировать точку - не расчитывать
        /// </summary>
        public bool IsIgnoredPoint { get; set; }
        public InsRequirementEnum InsValue { get; set; }
        public bool IsCalulated { get; set; }       

        public bool DefineSection (Map map)
        {
            bool res = false;
            if (IsIgnoredPoint) return res;

            if (Section == null)
            {
                Section = map.GetBuildingInPoint(Point);
                if (Section == null)
                {
                    IsIgnoredPoint = true;
                    res = false;
                }
                else
                {
                    res = true;
                }
            }
            else
            {
                res = true;
            }
            return res;            
        }        
    }
}