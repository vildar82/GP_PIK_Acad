using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Простая визуализация зон освещенности точки (без елочек)
    /// </summary>
    public interface IVisualInsPointSimple : IVisualService
    {
        void Visual (InsPoint insPoint);
    }
}
