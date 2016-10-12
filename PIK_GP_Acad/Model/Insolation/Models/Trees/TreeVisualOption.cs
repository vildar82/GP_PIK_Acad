using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Catel.Data;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    public class TreeVisualOption : ModelBase, INodDataSave
    {
        public Color Color { get; set; }
        public int Height { get; set; }

        public TreeVisualOption () { }

        public TreeVisualOption(Color color, int height) : base()
        {
            Color = color;
            Height = height;
        }

        public static ObservableCollection<TreeVisualOption> DefaultTreeVisualOptions ()
        {
            ObservableCollection<TreeVisualOption> visuals = new ObservableCollection<TreeVisualOption> {
                new TreeVisualOption (Color.FromArgb(205, 32, 39), 35),
                new TreeVisualOption (Color.FromArgb(241, 235, 31), 55),
                new TreeVisualOption (Color.FromArgb(19, 155, 72), 75),
            };
            return visuals;
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            throw new NotImplementedException();
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            throw new NotImplementedException();
        }
    }
}
