using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;

namespace PIK_GP_Acad.Elements
{
    /// <summary>
    /// Любой элемент генплана
    /// </summary>
    public interface IElement
    {
        Error Error { get; }
    }
}
