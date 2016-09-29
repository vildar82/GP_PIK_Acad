using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.GraphicsInterface;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Визуализируемый объект
    /// </summary>
    public interface IVisual
    {
        List<Drawable> CreateVisual (IVisualOptions options);
    }
}
