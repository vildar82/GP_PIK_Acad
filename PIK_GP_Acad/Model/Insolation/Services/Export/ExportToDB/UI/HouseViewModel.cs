using PIK_GP_Acad.Insolation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PIK_GP_Acad.Insolation.Services.Export
{
    public class HouseViewModel : ViewModelBase
    {
        // private House house;

        public HouseViewModel(House house)
        {
            Name = house.Name;
            if (house.SelectedHouseDb == null)
            {
                // Несвязанный дом - красным цветом
                Color = new SolidColorBrush( Colors.Red);
                Info = "Несвязанный дом с объектом базы. Не будет экспортирован.";
            }
            else
            {
                FullName = house.SelectedHouseDb.Name;                
            }            
        }

        public string Name { get; }
        public string FullName { get; }
        public string Info { get; }
        public Brush Color { get; }
    }
}
