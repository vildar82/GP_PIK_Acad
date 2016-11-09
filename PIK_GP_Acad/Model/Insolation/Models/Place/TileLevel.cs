using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Уровень освещенности ячейки
    /// </summary>
    public class TileLevel : IEquatable<TileLevel>
    {
        public TileLevel()
        {

        }

        public static TileLevel Empty { get; private set; } = new TileLevel { Color = Color.Gray, TotalTimeH = 0, Transparent = 0 };

        /// <summary>
        /// Общее время освещенности ячейки (в часах)
        /// </summary>        
        public double TotalTimeH {
            get { return totalTimeH; }
            set {
                totalTimeH = value;
                TotalTimeMin = totalTimeH.ToMin();
            }
        }
        double totalTimeH;
        public double TotalTimeMin { get; private set; }
        public Color Color { get; set; }
        public byte Transparent { get; set; }

        public bool Equals (TileLevel other)
        {
            if (other == null) return false;
            return TotalTimeH == other.TotalTimeH;
        }

        public override int GetHashCode ()
        {
            return TotalTimeH.GetHashCode();
        }
    }
}
