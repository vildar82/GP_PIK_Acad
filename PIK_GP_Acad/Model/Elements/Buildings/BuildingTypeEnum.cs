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
        [Catel.ComponentModel.DisplayName("Жилое")]
        Living,
        /// <summary>
        /// Социальное
        /// </summary>
        [Description("Социальное")]
        [Catel.ComponentModel.DisplayName("Социальное")]
        Social,
        /// <summary>
        /// Гараж
        /// </summary>
        [Description("Гараж")]
        [Catel.ComponentModel.DisplayName("Гараж")]
        Garage,
    }
}
