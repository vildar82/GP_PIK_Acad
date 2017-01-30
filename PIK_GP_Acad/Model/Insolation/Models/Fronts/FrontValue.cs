using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Фронт - полоса инсоляции одного значения
    /// </summary>
    public class FrontValue : IDisposable
    {
        public FrontValue ()
        {
        }

        public FrontValue (Point2d ptStart, Point2d ptEnd, InsRequirementEnum insValue, FrontOptions opt)
        {
            InsValue = insValue;
            Line = CreateLine(ptStart, ptEnd,  opt);                        
        }        

        /// <summary>
        /// Значение инсоляции
        /// </summary>
        public InsRequirementEnum InsValue { get; set; }
        /// <summary>
        /// Линия - фронт инсоляции этого значения
        /// </summary>
        public Polyline Line { get; set; }  

        private Polyline CreateLine (Point2d ptStart, Point2d ptEnd, FrontOptions opt)
        {
            var line = new Polyline(2);
            line.AddVertexAt(0, ptStart, 0, 0, 0);
            line.AddVertexAt(1, ptEnd, 0, 0, 0);
            line.Color = Color.FromColor(opt.GetFrontColor(InsValue));
            line.ConstantWidth = opt.LineFrontWidth;
            //line.LayerId = opt.FrontLineLayerId;
            return line;
        }

        /// <summary>
        /// Объединение полилиний фронтов
        /// Переданные линии объединяются. От объединенных линии берутся точки в объединяемую линию
        /// </summary>       
        public static List<FrontValue> Merge (ref List<FrontValue> fronts)
        {
            var mergedFronts = new List<FrontValue>();
            var prewFront = fronts.First();
            var firstFront = prewFront;
            foreach (var item in fronts.Skip(1))
            {
                if (item.InsValue == prewFront.InsValue &&
                    item.Line.StartPoint.IsEqualTo (prewFront.Line.EndPoint))
                {
                    prewFront.AddFront(item);
                    item.Dispose();
                }
                else
                {
                    mergedFronts.Add(prewFront);                    
                    prewFront = item;                    
                }
            }
            // Если последний сегмент и первый совпадают - то объединение            
            if (prewFront.InsValue == firstFront.InsValue &&
                !prewFront.Line.EndPoint.IsEqualTo(firstFront.Line.EndPoint) &&
                    prewFront.Line.EndPoint.IsEqualTo(firstFront.Line.StartPoint))
            {
                prewFront.Line.ReverseCurve();
                firstFront.Line.ReverseCurve();
                firstFront.AddFront(prewFront);
                prewFront.Dispose();            
            }
            else
            {
                mergedFronts.Add(prewFront);
            }
            return mergedFronts;
        }

        public FrontValue Clone ()
        {
            var resVal = new FrontValue();
            resVal.InsValue = InsValue;
            resVal.Line = (Polyline)Line.Clone();
            return resVal;
        }

        public void AddFront (FrontValue front)
        {
            var w = Line.ConstantWidth;
            int index = Line.NumberOfVertices;
            for (int i = 1; i < front.Line.NumberOfVertices; i++)
            {
                Line.AddVertexAt(index++, front.Line.GetPoint2dAt(i), 0,0,0);
            }            
            Line.ConstantWidth = w;            
        }

        public void Dispose()
        {
            Line?.Dispose();
        }
    }
}
