using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolaion.Constructions;

namespace PIK_GP_Acad.Insolaion.Constructions
{
    /// <summary>
    /// Создание зданий
    /// </summary>
    public static class BuildingFactory
    {
        public static IBuilding CreateBuilding(Entity ent)
        {
            IBuilding res = null;
            var pl = ent as Polyline;
            if (pl != null)
            {                
                var layer = pl.Layer.ToUpper();
                if (layer.Contains("ОКРУЖАЮЩАЯ") && layer.Contains("ЗАСТРОЙКА"))
                {
                    res = new Surround(pl);
                }
            }
            return res;
        }
    }
}
