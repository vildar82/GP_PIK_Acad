using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services.Export
{
    /// <summary>
    /// Дом - трансформация
    /// </summary>
    public class HouseTransform
    {
        private House house;

        public HouseTransform(House house)
        {
            this.house = house;
        }
    }
}
