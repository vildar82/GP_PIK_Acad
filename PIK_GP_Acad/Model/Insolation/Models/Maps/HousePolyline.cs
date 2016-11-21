using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Дом из классифицированной полилинии
    /// </summary>
    public class HouseSingle : House
    {
        public HouseSingle(FrontGroup frontGroup, MapBuilding buildingPl) : base(frontGroup)
        {
            Sections.Add(buildingPl);            
        }

        public override void DefineContour()
        {
            var buildingPl = Sections[0];
            buildingPl.InitContour();
            using (buildingPl.Contour)
            {
                Contour = (Polyline)buildingPl.Contour.Clone();
            }
        }
    }
}
