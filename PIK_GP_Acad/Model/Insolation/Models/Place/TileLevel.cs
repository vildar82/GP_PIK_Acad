using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Уровень освещенности ячейки
    /// </summary>
    public class TileLevel : ModelBase, IEquatable<TileLevel>, ITypedDataValues
    {
        public TileLevel()
        {

        }

        public static TileLevel Empty { get; private set; } = new TileLevel { Color = Color.Gray, TotalTimeH = 0};

        /// <summary>
        /// Общее время освещенности ячейки (в часах)
        /// </summary>        
        public double TotalTimeH {
            get { return totalTimeH; }
            set {
                totalTimeH = value;
                TotalTimeMin = totalTimeH.ToMin();
                RaisePropertyChanged();
            }
        }
        double totalTimeH;

        public double TotalTimeMin { get; private set; }

        public Color Color { get { return color; } set { color = value; RaisePropertyChanged(); } }
        Color color;

        public bool Equals (TileLevel other)
        {
            if (other == null) return false;
            return TotalTimeH == other.TotalTimeH;
        }

        public static ObservableCollection<TileLevel> Defaults ()
        {
            return new ObservableCollection<TileLevel>() {
                new TileLevel { TotalTimeH=3, Color = System.Drawing.Color.Yellow }
            };
        }

        public override int GetHashCode ()
        {
            return TotalTimeH.GetHashCode();
        }

        /// <summary>
        /// Проверка и корректировка уровней
        /// </summary>        
        public static List<TileLevel> CheckAndCorrect (List<TileLevel> levels)
        {
            // сортировка по уровням
            var levelsCorrect = levels.Where(w=>w.TotalTimeH>0).GroupBy(g => g.TotalTimeH)
                .Select(s => s.First()).OrderByDescending(o => o.TotalTimeH).ToList();
            return levelsCorrect;
        }        

        public List<TypedValue> GetDataValues (Document doc)
        {
            return new List<TypedValue> {
                TypedValueExt.GetTvExtData(TotalTimeH),
                TypedValueExt.GetTvExtData(Color.A),
                TypedValueExt.GetTvExtData(Color.R),
                TypedValueExt.GetTvExtData(Color.G),
                TypedValueExt.GetTvExtData(Color.B)
            };
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            if (values== null && values.Count != 5)
            {
                // Default
                TotalTimeH = 3;
                Color = Color.Yellow;
            }
            else
            {
                int index = 0;
                TotalTimeH = TypedValueExt.GetTvValue<double>(values[index++]);
                var a = values[index++].GetTvValue<byte>();
                var r = values[index++].GetTvValue<byte>();
                var g = values[index++].GetTvValue<byte>();
                var b = values[index++].GetTvValue<byte>();
                Color = Color.FromArgb(a, r, g, b);
            }
        }
    }
}
