using MicroMvvm;
using PIK_GP_Acad.Insolation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Services.Export
{
    public class GroupViewModel : ViewModelBase
    {
        //private FrontGroup front;

        public GroupViewModel(FrontGroup front)
        {
            //this.front = front;
            Houses = new List<HouseViewModel>(front.Houses.Select(s=>new HouseViewModel(s)));
            Name = front.Name;
        }

        public string Name { get; }
        public List<HouseViewModel> Houses { get; }
    }
}
