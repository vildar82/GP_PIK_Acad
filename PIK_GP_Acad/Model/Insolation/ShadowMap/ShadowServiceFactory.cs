
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation;

namespace PIK_GP_Acad.Model.Insolation.ShadowMap
{
    public static class ShadowServiceFactory
    {
        public static IShadowService Create(Map map)
        {
            IShadowService shadowService = null;

            if (map.Options.Region == Region.Central)
            {
                shadowService = new ShadowCentral(map);
            }

            return shadowService;
        }
    }
}
