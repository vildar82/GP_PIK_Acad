using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Расчет фронтонов
    /// </summary>
    public interface ICalcPlace
    {
        /// <summary>
        /// Расчет площадки
        /// </summary>
        List<Tile> CalcPlace (Place place);
    }
}
