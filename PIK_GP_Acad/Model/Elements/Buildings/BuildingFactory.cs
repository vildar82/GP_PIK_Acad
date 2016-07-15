using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.FCS;

namespace PIK_GP_Acad.Elements.Buildings
{
    /// <summary>
    /// Создание зданий
    /// </summary>
    public static class BuildingFactory
    {
        public static IBuilding CreateBuilding(Entity ent)
        {
            IBuilding res = null;

            if (ent is BlockReference)
            {
                var blRef = (BlockReference)ent;
            }

            var pl = ent as Polyline;
            if (pl != null)
            {
                KeyValuePair<string, List<FCProperty>> tag;
                if (FCService.GetTag(pl.Id, out tag))
                {
                    // Если есть параметр высоты
                    var height = FCService.GetPropertyValue<int>(Building.PropHeight, tag.Value, pl.Id, false);
                    if (height != 0)
                    {
                        res = new Surround(pl, height, tag.Value);
                    }
                }               
            }
            return res;
        }

        
    }
}
