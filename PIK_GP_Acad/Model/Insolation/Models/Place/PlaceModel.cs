﻿using System;
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
using AcadLib.Errors;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчет площадок - освещенностей на выбранных участках (полилинии)
    /// </summary>
    public class PlaceModel : ModelBase, IExtDataSave, ITypedDataValues, IDisposable
    {
        public PlaceModel()
        {
            IsEnableCalc = false;
            Places = new ObservableCollection<Place>();
        }

        public InsModel Model { get; set; }

        public ObservableCollection<Place> Places { get { return places;  } set { places = value; RaisePropertyChanged(); } }
        private ObservableCollection<Place> places;

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
            AddPlacesFromMap();                     
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
            var place = FindPlace(plId);
            if (place != null) return; // Такая площадка уже есть.
            place = new Place(plId, dicPlace, this);
            Places.Add(place);            
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

        public void Update()
        {
            try
            {
                AddPlacesFromMap();

                if (!IsEnableCalc) return;

                // Очистка удаленных контуров
                var deletedPlaces = Places.Where(p => !p.PlaceId.IsValidEx()).ToList();
                foreach (var item in deletedPlaces)
                {
                    Places.Remove(item);
                    item.Dispose();
                }
                foreach (var place in Places)
                {
                    place.Update();
                }
            }
            catch(Exception ex)
            {
                Inspector.AddError($"Ошибка расчета площадок - {ex.Message}");
                Logger.Log.Error(ex, "PlaceModel.Update()");
            }
        }

        public void AddPlacesFromMap()
        {
            DisposePlaces();
            Places = new ObservableCollection<Place>();
            var placesInMap = Model.Map?.Places;
            if (placesInMap != null)
            {
                // Загрузка площадок из карты
                foreach (var placeId in Model.Map.Places)
                {
                    if (placeId.Key.IsValidEx())
                    {
                        AddPlace(placeId.Key, placeId.Value);
                    }                    
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
            if (Places == null) return;
            foreach (var item in Places)
            {
                item?.ClearVisual();
            }
        }

        public void DrawVisuals()
        {            
            if (Places != null)
            {
                var visuals = new List<IVisualService>();
                foreach (var place in Places)
                {               
                    if (place?.VisualPlace != null)     
                        visuals.Add(place.VisualPlace);
                }
                VisualDatabaseAny.DrawVisualsForUser(visuals);
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
            //tvk.Add("IsEnableCalc", IsEnableCalc);            
            IsEnableCalc = false;// Долго считаются площадки. Пусть сразу никогда не считаются.
            return tvk.Values;
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            var dictValues = values?.ToDictionary();
            IsEnableCalc = false;// dictValues.GetValue("IsEnableCalc", false); // Долго считаются площадки. Пусть сразу никогда не считаются.              
        }

        public void DisposePlaces ()
        {
            if (Places != null)
            {
                foreach (var item in Places)
                {
                    item.Dispose();
                }
            }
        }
        public void Dispose()
        {
            DisposePlaces();
        }
    }
}
