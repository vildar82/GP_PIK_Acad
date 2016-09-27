
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Model.Insolation.ShadowMap
{
    public static class ShadowServiceFactory
    {
        public static IShadowService Create(Map map)
        {
            IShadowService shadowService = null;

            //if (map.Options.Region.RegionPart == RegionEnum.Central)
            //{
            //    shadowService = new ShadowCentral(map);
            //}

            return shadowService;
        }
    }
}
