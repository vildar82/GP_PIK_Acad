using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolaion.SunlightRule
{
    public interface ISunlightRule
    {
        /// <summary>
        /// Определение длины для высоты
        /// </summary>
        /// <param name="height">Высота, м.</param>
        /// <returns>Радиус, м.</returns>
        int GetLength (int height);
    }
}
