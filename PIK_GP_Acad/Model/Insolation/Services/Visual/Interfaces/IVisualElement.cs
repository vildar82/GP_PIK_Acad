using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Элемент который может визуализироваться
    /// </summary>
    public interface IVisualElement
    {
        /// <summary>
        /// Создание элементов визуализации
        /// </summary>        
        List<Entity> CreateVisual();
    }
}
