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
    public class InsRequirement : ModelBase
    {
        InsRequirementEnum type;
        Color color;
        string name;

        public InsRequirementEnum Type {
            get { return type; }
            set {
                if (type != value)
                {
                    type = value;
                    RaisePropertyChanged();
                    Name = AcadLib.WPF.Converters.EnumDescriptionTypeConverter.GetEnumDescription(Type);                    
                }
            }
        }
        public Color Color {
            get { return color; }
            set {
                if (color != value)
                {
                    color = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string Name { get { return name; } set { name = value; RaisePropertyChanged(); } }
    }
}
