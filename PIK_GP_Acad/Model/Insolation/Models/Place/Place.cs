using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib;
using Autodesk.AutoCAD.EditorInput;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Одна площадка - по контуру полилинии
    /// </summary>
    public class Place : ModelBase, IDisposable
    {
        public Place (ObjectId placeId, PlaceModel placeModel)
        {
            PlaceModel = placeModel;
            PlaceId = placeId;
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
                isVisualPlaceOn = true;
                RaisePropertyChanged();
                OnVisualPlaceChanged();
            }
        }

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

        private string GetLevelsInfo (List<Tile> tiles)
        {
            var groupTiles = tiles.GroupBy(g => g.Level.TotalTimeH).OrderByDescending(o=>o.Key)
                .Select(s=> $"{s.Key}ч.-{s.Sum(i=>i.Area).Round(2)}м{General.Symbols.Square}");
            var levelsInfo = string.Join(", ", groupTiles);
            return levelsInfo;
        }

        private void OnVisualPlaceChanged ()
        {
            VisualPlace.VisualIsOn = IsVisualPlaceOn;
        }

        private string DefineName ()
        {
            string name;
            // TODO: Загрузить словарь полилинии площадки

            name = $"Площадка {PlaceModel.Places.Count + 1}";
            return name;
        }

        public void Dispose ()
        {
            VisualPlace.VisualsDelete();
            VisualPlace.DisposeTiles();
            VisualPlace.Dispose();
        }
    }
}
