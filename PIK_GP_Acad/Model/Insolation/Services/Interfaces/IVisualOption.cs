using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PIK_GP_Acad.Insolation.Services
{
    public interface IVisualOptions
    {
        Color Color { get; set; }
        byte Transparency { get; set; }
    }
}
