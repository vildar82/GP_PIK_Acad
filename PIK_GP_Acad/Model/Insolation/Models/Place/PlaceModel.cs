using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Services;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчет площадок - освещенностей на выбранных участках (полилинии)
    /// </summary>
    public class PlaceModel : ModelBase, IExtDataSave, ITypedDataValues, IDisposable
    {
        public PlaceModel()
        {            
        }

        public InsModel Model { get; set; }

        public ObservableCollection<Place> Places { get; set; } = new ObservableCollection<Place>();

        public PlaceOptions Options { get; set; }
        /// <summary>
        /// Включен/выключен расчет площадок.
        /// </summary>
        public bool IsEnableCalc { get { return isEnableCalc; } set { isEnableCalc = value; RaisePropertyChanged(); } }
        bool isEnableCalc;

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

        public void AddPlace (ObjectId plId, DicED dicPlace)
        {
            if (!plId.IsValidEx()) return;
            var place = new Place(plId, dicPlace, this);
            Places.Add(place);
            place.Update();
        }

        public void DeletePlace (Place place)
        {
            // Удаление словаря инсоляции.
            place.Delete();            
            Places.Remove(place);
        }

        private Place FindPlace (ObjectId placeId)
        {
            var findPlace = Places.FirstOrDefault(p => p.PlaceId == placeId);
            return findPlace;
        }

        public void Update ()
        {
            if (!isEnableCalc) return;

            if (Places.Count == 0)
            {
                var placesId = Model.Map?.Places;
                if (placesId != null)
                {
                    // Загрузка площадок из карты
                    foreach (var placeId in Model.Map.Places)
                    {
                        AddPlace(placeId.Key, placeId.Value);
                    }
                }
            }
            else
            {
                // Очистка удаленных контуров
                var deletedPlaces = Places.Where(p => !p.PlaceId.IsValidEx()).ToList();
                foreach (var item in deletedPlaces)
                {
                    item.Delete();
                    Places.Remove(item);
                }
                foreach (var place in Places)
                {                    
                    place.Update();
                }
            }
        }

        public void UpdateVisual()
        {
            foreach (var item in Places)
            {
                item.UpdateVisual();
            }
        }

        public void ClearVisual ()
        {
            foreach (var item in Places)
            {
                item.ClearVisual();
            }
        }

        public DicED GetExtDic (Document doc)
        {
            var dicPlace = new DicED();
            dicPlace.AddInner("Options", Options.GetExtDic(doc));
            dicPlace.AddRec("PlaceRec", GetDataValues(doc));
            // Сохранение площадок            
            foreach (var item in Places)
            {
                item.Save();
            }

            return dicPlace;
        }

        public void SetExtDic (DicED dicPlace, Document doc)
        {
            Options = new PlaceOptions();
            Options.SetExtDic(dicPlace?.GetInner("Options"), doc);
            SetDataValues(dicPlace?.GetRec("PlaceRec")?.Values, doc);
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            var tvk = new TypedValueExtKit();
            tvk.Add("IsEnableCalc", IsEnableCalc);            
            return tvk.Values;
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            var dictValues = values?.ToDictionary();
            IsEnableCalc = dictValues.GetValue("IsEnableCalc", true);            
        }

        public void Dispose ()
        {
            if (Places != null)
            {
                foreach (var item in Places)
                {
                    item.Dispose();
                }
            }
        }        
    }
}
