using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Catel.Fody;
using Catel.MVVM;
using PIK_GP_Acad.Insolation.Trees;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsPointViewModel : ViewModelBase
    {
        Random rnd = new Random();

        public InsPointViewModel (IInsPoint insPoint): base()
        {            
            InsPoint = insPoint;
            Color = Color.FromRgb((byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255));
        }

        [Model]
        [Expose("Number")]
        public IInsPoint InsPoint { get; set; }        

        public Color Color { get; set; }

    }
}
