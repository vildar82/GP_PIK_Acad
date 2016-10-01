using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Services
{
    public interface IVisualService
    {
        /// <summary>
        /// Включение/выключение визуализации
        /// </summary>
        bool IsOn { get; set; }
        void CreateVisual (object model);
    }
}
