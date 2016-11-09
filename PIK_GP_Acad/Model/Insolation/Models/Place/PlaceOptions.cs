using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Настройки площадок
    /// </summary>
    public class PlaceOptions : ModelBase
    {
        public PlaceOptions()
        {

        }

        /// <summary>
        /// Размер ячейки площадки (шаг расчетных точек на площадке) [м]
        /// </summary>
        public double TileSize { get; set; } = 0.1;

        public List<TileLevel> Levels { get; set; }

        public static PlaceOptions Default ()
        {
            var opt = new PlaceOptions();
            opt.Levels = new List<TileLevel>() {
                new TileLevel { TotalTimeH=3, Color = System.Drawing.Color.Yellow, Transparent = 60 }
            };
            return opt;
        }
    }
}
