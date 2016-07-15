using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.FCS;

namespace PIK_GP_Acad.Elements
{
    /// <summary>
    /// Площадной классифиципуемый элемент
    /// </summary>
    public interface IArea : IClassificator, IElement
    {
        double Area { get; }
    }
}
