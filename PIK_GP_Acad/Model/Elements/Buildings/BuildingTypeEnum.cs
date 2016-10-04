using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Elements.Buildings
{
    public enum BuildingTypeEnum
    {
        [Description("Жилое")]
        [Catel.ComponentModel.DisplayName("Жилое")]
        Living,
        [Description("Социальное")]
        [Catel.ComponentModel.DisplayName("Социальное")]
        Social,
        [Description("Гараж")]
        [Catel.ComponentModel.DisplayName("Гараж")]
        Garage,
    }
}
