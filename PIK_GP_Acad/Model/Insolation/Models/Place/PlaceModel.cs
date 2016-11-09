using System;
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

        public void AddPlace (ObjectId placeId)
        {
            // Проверка нет ли уже такой площадки (по placeId)
            var place = GetPlace(placeId);
            if (place != null)
            {
                InsService.ShowMessage("Эта площадка уже добавлена - " + place.Name, System.Windows.MessageBoxImage.Information);
                return;
            }
            place = new Place(placeId, this);
            Places.Add(place);
            place.Update();
        }

        private Place GetPlace (ObjectId placeId)
        {
            throw new NotImplementedException();
        }
    }
}
