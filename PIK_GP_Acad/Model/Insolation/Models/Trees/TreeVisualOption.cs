using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Catel.Data;

namespace PIK_GP_Acad.Insolation.Models
{
    public class TreeVisualOption : ModelBase
    {
        public Color Color { get; set; }
        public int Height { get; set; }

        public TreeVisualOption () { }

        public TreeVisualOption(Color color, int height) : base()
        {
            Color = color;
            Height = height;
        }

        
    }
}
