using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Настройки инсоляционного требования
    /// </summary>
    public class InsRequirement
    {
        public InsRequirementEnum Type { get; set; }
        public Color Color { get; set; }
        public string Name { get { return AcadLib.WPF.Converters.EnumDescriptionTypeConverter.GetEnumDescription(Type); } }
    }
}
