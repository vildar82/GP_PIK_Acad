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
            VisualPlace = new VisualPlace(placeModel);
            SetExtDic(dicPlace, Application.DocumentManager.MdiActiveDocument);
        }

        public Place (ObjectId plId, PlaceModel placeModel)
        {
            PlaceModel = placeModel;
            PlaceId = plId;
            VisualPlace = new VisualPlace(placeModel);
            Name = DefineName();            
        }        

        public PlaceModel PlaceModel { get; set; }

        public ObjectId PlaceId { get; set; }
        /// <summary>
        ///  Площадь площадки м2
        /// </summary>
        public double Area { get { return area; } set { area = value; RaisePropertyChanged(); } }
        double area;
              
        public List<Tile> Tiles { get; set; }
        public string LevelsInfo { get { return levelsInfo; } set { levelsInfo = value; RaisePropertyChanged(); } }
        string levelsInfo;

        public string Name { get { return name; } set { name = value; RaisePropertyChanged(); } }
        string name;        

        public VisualPlace VisualPlace { get; set; }        

        public bool IsVisualPlaceOn { get { return isVisualPlaceOn; }
            set {
                isVisualPlaceOn = value;
                RaisePropertyChanged();
                OnVisualPlaceChanged();
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
            Tiles = PlaceModel.Model.CalcService.CalcPlace.CalcPlace(this);
            // Суммирование освещенностей по уровням
            LevelsInfo = GetLevelsInfo(Tiles);
            // Визуализация ячеек
            VisualPlace.Tiles = Tiles;
            VisualPlace.VisualUpdate();
        }

        public void ClearVisual ()
        {
            VisualPlace?.VisualsDelete();
        }

        private string GetLevelsInfo (List<Tile> tiles)
        {
            var groupTiles = tiles.GroupBy(g => g.Level.TotalTimeH).OrderByDescending(o=>o.Key)
                .Select(s=> $"{s.Key}ч.-{s.Sum(i=>i.Area).Round(2)}м{General.Symbols.Square}");
            var levelsInfo = string.Join(", ", groupTiles);
            return levelsInfo;
        }

        private void OnVisualPlaceChanged ()
        {
            if (VisualPlace != null)
            {
                VisualPlace.VisualIsOn = IsVisualPlaceOn;
            }
        }

        private string DefineName ()
        {
            string name;
            // TODO: Загрузить словарь полилинии площадки

            name = $"Площадка {PlaceModel.Places.Count + 1}";
            return name;
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

        public void Dispose ()
        {
            VisualPlace?.VisualsDelete();
            VisualPlace?.DisposeTiles();
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
            return new List<TypedValue> {
                TypedValueExt.GetTvExtData(Name),
                TypedValueExt.GetTvExtData(IsVisualPlaceOn)
            };
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            if (values == null || values.Count != 2)
            {
                // Default
                Name = "Площадка " + PlaceModel?.Places.Count; 
            }
            else
            {
                Name = TypedValueExt.GetTvValue<string>(values[0]);
                IsVisualPlaceOn = TypedValueExt.GetTvValue<bool>(values[1]);
            }
        }
    }
}
