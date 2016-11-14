using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public double TileSize { get { return tileSize; } set { tileSize = value; RaisePropertyChanged(); } }
        double tileSize;

        public ObservableCollection<TileLevel> Levels { get { return levels; } set { levels = value; RaisePropertyChanged(); } }
        ObservableCollection<TileLevel> levels;

        public byte Transparent { get; set; } = 60;

        public static PlaceOptions Default ()
        {
            var opt = new PlaceOptions();
            opt.TileSize = 1;
            opt.Levels = new ObservableCollection<TileLevel>() {
                new TileLevel { TotalTimeH=3, Color = System.Drawing.Color.Yellow }
            };
            return opt;
        }
    }
}
