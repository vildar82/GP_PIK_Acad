using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroMvvm;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.UI
{
    public class PlaceViewModel : ViewModelBase
    {
        public PlaceViewModel(PlaceModel place)
        {
            Place = place;
            Add = new RelayCommand(OnAddExecute);
            Show = new RelayCommand<Place>(OnShowExecute);
        }        

        /// <summary>
        /// Модель
        /// </summary>
        public PlaceModel Place { get; set; }

        public RelayCommand Add { get; set; }
        public RelayCommand<Place> Show { get; set; }

        /// <summary>
        /// Добавление новой площадки
        /// </summary>
        private void OnAddExecute ()
        {
            var selPlace = new SelectPlace();
            var placeId = selPlace.Select();
            Place.AddPlace(placeId);
        }

        /// <summary>
        /// Показать площадку на чертеже
        /// </summary>
        private void OnShowExecute (Place place)
        {
            place.Show();
        }
    }
}
