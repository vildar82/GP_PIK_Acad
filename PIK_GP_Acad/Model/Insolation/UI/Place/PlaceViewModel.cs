using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroMvvm;
using PIK_GP_Acad.Insolation.Models;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.UI
{
    public class PlaceViewModel : ViewModelBase
    {
        public PlaceViewModel(PlaceModel place)
        {
            Place = place;
            Add = new RelayCommand(OnAddExecute);
            EditOptions = new RelayCommand(OnEditOptionsExecute);
            Show = new RelayCommand<Place>(OnShowExecute);
            Delete = new RelayCommand<Place>(OnDeleteExecute);
        }

        /// <summary>
        /// Модель
        /// </summary>
        public PlaceModel Place { get; set; }

        public RelayCommand Add { get; set; }
        public RelayCommand EditOptions { get; set; }
        public RelayCommand<Place> Show { get; set; }
        public RelayCommand<Place> Delete { get; set; }

        /// <summary>
        /// Добавление новой площадки
        /// </summary>
        private void OnAddExecute ()
        {
            var selPlace = new SelectPlace();
            var placeId = selPlace.Select();
            if (placeId.IsNull) return;
            var place = Place.AddPlace(placeId);
            if (place != null)
            {
                place.IsVisualPlaceOn = true;
            }
        }

        /// <summary>
        /// Показать площадку на чертеже
        /// </summary>
        private void OnShowExecute (Place place)
        {
            place.Show();
        }

        private void OnEditOptionsExecute ()
        {
            var placeOptVM = new PlaceOptionsViewModel(Place.Options);
            InsService.ShowDialog(placeOptVM);
        }

        private void OnDeleteExecute (Place place)
        {
            Place.DeletePlace(place);
        }
    }
}
