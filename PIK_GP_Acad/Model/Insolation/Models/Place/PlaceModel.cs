﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчет площадок - освещенностей на выбранных участках (полилинии)
    /// </summary>
    public class PlaceModel : ModelBase
    {
        public PlaceModel()
        {            
        }

        public InsModel Model { get; set; }

        public ObservableCollection<Place> Places { get; set; } = new ObservableCollection<Place>();

        public PlaceOptions Options { get; set; }

        public void Initialize (InsModel insModel)
        {
            Model = insModel;

            if (Options == null)
                Options = PlaceOptions.Default();                        
        }

        public Place AddPlace (ObjectId placeId)
        {
            // Проверка нет ли уже такой площадки (по placeId)
            var place = FindPlace(placeId);
            if (place != null)
            {
                InsService.ShowMessage("Эта площадка уже добавлена - " + place.Name, System.Windows.MessageBoxImage.Information);
                return null;
            }
            place = new Place(placeId, this);
            Places.Add(place);
            place.Update();
            return place;
        }

        public void RemovePlace (Place place)
        {
            place.Dispose();
            Places.Remove(place);
        }

        private Place FindPlace (ObjectId placeId)
        {
            var findPlace = Places.FirstOrDefault(p => p.PlaceId == placeId);
            return findPlace;
        }

        public void Update ()
        {
            foreach (var place in Places)
            {
                place.Update();
            }
        }        
    }
}
