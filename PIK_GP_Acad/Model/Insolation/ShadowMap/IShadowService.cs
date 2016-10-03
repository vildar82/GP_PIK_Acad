using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Elements.Buildings;

namespace PIK_GP_Acad.Model.Insolation.ShadowMap
{
    public interface IShadowService
    {
        /// <summary>
        /// Расчет тени от здания
        /// </summary>        
        Shadow Calc (IBuilding building);
    }
}
