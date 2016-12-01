using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services.Export
{
    /// <summary>
    /// Ячейка инсоляции
    /// </summary>
    public class InsCell
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public InsRequirementEnum InsValue { get; set; }

        public Point2d PtCenter { get; set; }
    }
}
