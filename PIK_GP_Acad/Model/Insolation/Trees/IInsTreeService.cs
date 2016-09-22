using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation
{
    /// <summary>
    /// Расчет Елочек (освещенности в заданных точках)
    /// </summary>
    public interface IInsTreeService
    {
        /// <summary>
        /// Добавление расчетной точки
        /// </summary>        
        IInsPoint AddPoint ();
    }
}
