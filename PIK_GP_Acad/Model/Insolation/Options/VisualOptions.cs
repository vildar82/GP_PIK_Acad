using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PIK_GP_Acad.Insolation.Options
{
    public class VisualOption
    {
        public Color Color { get; set; }
        public int Height { get; set; }
        //public Color ColorLow { get; set; } = Color.FromRgb(205, 32, 39);
        //public Color ColorMedium { get; set; } = Color.FromRgb(241, 235, 31);
        //public Color ColorHight { get; set; } = Color.FromRgb(19, 155, 72);
        //public int HeightLow { get; set; } = 35;
        //public int HeightMedium { get; set; } = 55;
        //public int HeightMax { get; set; } = 75;

        public VisualOption(Color color, int height)
        {
            Color = color;
            Height = height;
        }
    }
}
