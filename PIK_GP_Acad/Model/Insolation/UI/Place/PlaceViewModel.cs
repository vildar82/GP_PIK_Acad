using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroMvvm;
using PIK_GP_Acad.Insolation.Models;
using PIK_GP_Acad.Insolation.Services;
using AcadLib;
using AcadLib.Statistic;
using Autodesk.AutoCAD.ApplicationServices;

namespace PIK_GP_Acad.Insolation.UI
{
    public class PlaceViewModel : ViewModelBase
    {
        public PlaceViewModel(PlaceModel place)
        {
            Place = place;
            Add = new RelayCommand(OnAddPlaceExecute);
            EditOptions = new RelayCommand(OnEditOptionsExecute);
            ShowPlace = new RelayCommand<Place>(OnShowPlaceExecute);
            Delete = new RelayCommand<Place>(OnDeleteExecute);
        }        

        /// <summary>
        /// Модель
        /// </summary>
        public PlaceModel Place { get; set; }

        public RelayCommand Add { get; set; }
        public RelayCommand EditOptions { get; set; }
        public RelayCommand<Place> ShowPlace { get; set; }
        public RelayCommand<Place> Delete { get; set; }

        /// <summary>
        /// Добавление новой площадки
        /// </summary>
        private void OnAddPlaceExecute ()
        {
            var selPlace = new SelectPlace();
            var placeId = selPlace.Select();
            if (!placeId.IsValidEx()) return;
            var place = Place.AddPlace(placeId);
            if (place != null)
            {
                place.IsVisualPlaceOn = true;
            }
            // Запись статистики
            PluginStatisticsHelper.AddStatistic();
        }

        /// <summary>
        /// Показать площадку на чертеже
        /// </summary>
        private void OnShowPlaceExecute (Place place)
        {
            if ((short)Application.GetSystemVariable("TILEMODE") == 0) return;
            place.Show();
        }

        private void OnEditOptionsExecute ()
        {
            var placeOptVM = new PlaceOptionsViewModel(Place.Options);
            if (InsService.ShowDialog(placeOptVM) == true)
            {
                Place.Update();
            }
        }

        private void OnDeleteExecute (Place place)
        {
            Place.DeletePlace(place);
        }
    }
}
