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
    public class FrontValue
    {
        public FrontValue (Point2d ptStart, Point2d ptEnd, InsRequirementEnum insValue, FrontOptions opt)
        {            
            Line = CreateLine(ptStart, ptEnd,  opt);            
            InsValue = insValue;
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
            line.AddVertexAt(1, ptStart, 0, 0, 0);
            line.Color = Color.FromColor(opt.GetFrontColor(InsValue));
            line.ConstantWidth = opt.LineFrontWidth;
            line.LayerId = opt.FrontLineLayerId;
            return line;
        }
    }
}
