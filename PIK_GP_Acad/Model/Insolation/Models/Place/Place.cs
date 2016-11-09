using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib;
using Autodesk.AutoCAD.EditorInput;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Одна площадка - по контуру полилинии
    /// </summary>
    public class Place : ModelBase
    {
        public Place (ObjectId placeId, PlaceModel placeModel)
        {
            PlaceModel = placeModel;
            PlaceId = placeId;
        }

        public PlaceModel PlaceModel { get; set; }

        public ObjectId PlaceId { get; set; }

        public List<Tile> Tiles { get; set; }
        public string LevelsInfo { get { return levelsInfo; } set { levelsInfo = value; RaisePropertyChanged(); } }
        string levelsInfo;

        public string Name { get { return name; } set { name = value; RaisePropertyChanged(); } }
        string name;        

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
            
        }

        private string GetLevelsInfo (List<Tile> tiles)
        {
            var groupTiles = tiles.GroupBy(g => g.Level.TotalTimeH).OrderByDescending(o=>o.Key)
                .Select(s=> $"{s.Key}ч. = {s.Sum(i=>i.Area)}");
            var levelsInfo = string.Join(";", groupTiles);
            return levelsInfo;
        }
    }
}
