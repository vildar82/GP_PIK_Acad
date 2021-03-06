﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib;
using Autodesk.AutoCAD.EditorInput;
using PIK_GP_Acad.Insolation.Services;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using MicroMvvm;
using AcadLib.Errors;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Одна площадка - по контуру полилинии
    /// </summary>
    public class Place : ModelBase, IDisposable, AcadLib.XData.IDboDataSave
    {
        public const string PlaceDicName = "Place";        

        /// <summary>
        /// Для загрузки площадки из расш.данных полилинии
        /// </summary>
        public Place (ObjectId plId, DicED dicPlace, PlaceModel placeModel)
        {
            PlaceModel = placeModel;
            PlaceId = plId;
            //VisualPlace = new VisualPlace(placeModel);
            SetExtDic(dicPlace, Application.DocumentManager.MdiActiveDocument);
        }

        public Place (ObjectId plId, PlaceModel placeModel)
        {
            PlaceModel = placeModel;
            PlaceId = plId;
            //VisualPlace = new VisualPlace(placeModel);
            DefineName();            
        }        

        public PlaceModel PlaceModel { get; set; }

        public ObjectId PlaceId { get; set; }
        /// <summary>
        ///  Площадь площадки м2
        /// </summary>
        public double Area { get { return area; } set { area = value; RaisePropertyChanged(); } }
        double area;
              
        //public List<Tile> Tiles { get; set; }
        public string LevelsInfo { get { return levelsInfo; } set { levelsInfo = value; RaisePropertyChanged(); } }
        string levelsInfo;

        public string Name { get { return name; } set { name = value; RaisePropertyChanged(); } }
        string name;        

        public VisualPlace VisualPlace { get; set; }                

        public bool IsVisualPlaceOn { get { return isVisualPlaceOn; }
            set {
                isVisualPlaceOn = value;
                RaisePropertyChanged();
                OnIsVisualPlaceChanged();
            }
        }

        public string PluginName { get; set; } = InsService.PluginName;
        bool isVisualPlaceOn;

        /// <summary>
        /// Показать площадку на чертеже
        /// </summary>
        public void Show ()
        {
            PlaceId.ShowEnt();
        }

        /// <summary>
        /// Обновление площадки - пересчет и перерисовка
        /// </summary>
        public void Update ()
        {
            try
            {
                if (!PlaceModel.IsEnableCalc) return;
                if (!IsVisualPlaceOn)
                {
                    VisualPlace?.Dispose();
                    VisualPlace = null;
                    return;
                }
                var tiles = PlaceModel.Model.CalcService.CalcPlace.CalcPlace(this);
                // Суммирование освещенностей по уровням
                LevelsInfo = GetLevelsInfo(tiles);
                // Визуализация ячеек
                if (VisualPlace == null)
                    VisualPlace = new VisualPlace(this);
                VisualPlace.Tiles = tiles;
                VisualPlace.VisualIsOn = true;
            }
            catch(Exception ex)
            {
                Inspector.AddError($"Ошибка расчета площадки {Name} - {ex.Message}", PlaceId, System.Drawing.SystemIcons.Error);
                Logger.Log.Error(ex, "Place.Update()");
            }
        }

        public void ClearVisual ()
        {
            VisualPlace?.VisualsDelete();
        }

        private string GetLevelsInfo (List<Tile> tiles)
        {
            if (tiles == null) return null;
            try
            {
                var groupTiles = tiles.GroupBy(g => g.Level.TotalTimeH).OrderByDescending(o => o.Key)
                    .Select(s => $"{s.Key}ч.-{NetLib.DoubleExt.Round(s.Sum(i => i.Area), 2)}м{General.Symbols.Square}");
                var levelsInfo = string.Join(", ", groupTiles);
                var minLevel = PlaceModel.Options.Levels.Min(o => o.TotalTimeH);
                levelsInfo = levelsInfo.Replace("0ч.", $"<{minLevel}ч.");
                return levelsInfo;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "Place.GetLevelsInfo()");
                return null;
            }
        }        

        private void OnIsVisualPlaceChanged ()
        {            
            if (VisualPlace != null && VisualPlace.Tiles != null)
            {
                VisualPlace.VisualIsOn = IsVisualPlaceOn;
            }
            else
            {
                Update();
            }            
        }

        private void DefineName ()
        {
            //string name;
            // TODO: Загрузить словарь полилинии площадки
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                using (doc.LockDocument())
                {
                    this.LoadDboDict();
                }
            }
            if (string.IsNullOrEmpty(Name))
            {
                Name = $"Площадка {PlaceModel.Places.Count + 1}";
            }
            //return name;
        }

        /// <summary>
        /// Сохранение площадки (в расш данных полилнии)
        /// </summary>
        public void Save()
        {
            this.SaveDboDict();
        }

        /// <summary>
        /// Удаление площадки - очистка словаря и удаление визуализации
        /// </summary>
        public void Delete ()
        {
            if (PlaceId.IsValidEx())
            {
                // Удаление словаря
                using (PlaceModel.Model.Doc.LockDocument())
                {
                    this.DeleteDboDict();
                }
            }
            Dispose();
        }

        public void UpdateVisual()
        {
            VisualPlace?.VisualUpdate();
        }

        public void Dispose ()
        {            
            VisualPlace?.Dispose();
        }

        public DBObject GetDBObject ()
        {
            if (PlaceId.IsValidEx())
            {
                return PlaceId.Open(OpenMode.ForWrite, false, true) as Polyline;
            }
            return null;
        }

        public DicED GetExtDic (Document doc)
        {
            var dicPlace = new DicED(PlaceDicName);
            dicPlace.AddRec("Recs", GetDataValues(doc));
            return dicPlace;
        }

        public void SetExtDic (DicED dicPlace, Document doc)
        {
            SetDataValues(dicPlace?.GetRec("Recs")?.Values, doc);
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            var tvk = new TypedValueExtKit();
            tvk.Add("Name", Name);
            tvk.Add("IsVisualPlaceOn", IsVisualPlaceOn);
            return tvk.Values;            
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            var dictValues = values?.ToDictionary();
            Name = dictValues.GetValue("Name", $"Площадка {(PlaceModel?.Places?.Count + 1) ?? 1}");
            isVisualPlaceOn = dictValues.GetValue("IsVisualPlaceOn", true);            
        }
    }
}
