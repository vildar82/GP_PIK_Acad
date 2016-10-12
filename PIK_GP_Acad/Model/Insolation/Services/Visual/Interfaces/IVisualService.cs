using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.GraphicsInterface;

namespace PIK_GP_Acad.Insolation.Services
{
    public interface IVisualService
    {
        /// <summary>
        /// Включение/выключение визуализации
        /// </summary>
        bool VisualIsOn { get; set; }

        /// <summary>
        /// Обновление визуализации
        /// </summary>
        void VisualUpdate ();
        List<Drawable> CreateVisual ();
    }
}
