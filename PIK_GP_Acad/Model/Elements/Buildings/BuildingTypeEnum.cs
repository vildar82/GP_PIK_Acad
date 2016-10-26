using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Elements.Buildings
{
    /// <summary>
    /// Типы зданий
    /// </summary>
    public enum BuildingTypeEnum
    {
        /// <summary>
        /// Жилое
        /// </summary>
        [Description("Жилое")]        
        Living,
        /// <summary>
        /// Социальное
        /// </summary>
        [Description("Социальное")]        
        Social,
        /// <summary>
        /// Гараж
        /// </summary>
        [Description("Гараж")]        
        Garage,
    }
}
