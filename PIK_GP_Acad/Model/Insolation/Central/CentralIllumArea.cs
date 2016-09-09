using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation.Central
{
    class CentralIllumArea : IlluminationArea
    {
        public CentralIllumArea (IInsolationService insService, double angleStart, double angleEnd, Point2d pt) 
            : base(insService, angleStart, angleEnd, pt)
        {
        }
    }
}
