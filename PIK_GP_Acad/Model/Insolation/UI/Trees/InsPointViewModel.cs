using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.UI.Trees
{
    public class InsPointViewModel : ObservableObject
    {
        Random rnd = new Random();
        IInsPoint insPoint;
        Brush color;        

        public InsPointViewModel (IInsPoint insPoint)
        {
            this.insPoint = insPoint;
            color = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255)));
        }

        public int Number {
            get { return insPoint.Number; }
            set { insPoint.Number = value;
                RaisePropertyChanged();
            }
        }

        public Brush Color {
            get { return color; }
            set { color = value;
                RaisePropertyChanged();
            }
        }

    }
}
