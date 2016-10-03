using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Регион. Влияет на алгоритм расчета теней.
    /// </summary>
    public enum RegionEnum
    {
        /// <summary>
        /// Центральный - расчет по плоскости (лучевой конус в день равноденствия превращается в плоскость)
        /// </summary>
        Central,
        /// <summary>
        /// Северный - лучевой конус
        /// </summary>
        North,
        /// <summary>
        /// Южный - лучевой конус
        /// </summary>
        South                
    }
}
