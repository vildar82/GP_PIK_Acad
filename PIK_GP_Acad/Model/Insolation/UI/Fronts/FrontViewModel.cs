using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.UI
{
    public class FrontViewModel : ViewModelBase
    {
        public FrontViewModel (FrontModel model)
        {
            this.front = model;
        }

        FrontModel front;
    }
}
