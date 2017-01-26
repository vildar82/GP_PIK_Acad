using AcadLib.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLib;
using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Дома на карте
    /// </summary>
    public class HouseMap : IEnumerable<House>
    {
        private Map map;
        private List<House> houses;
        private int indexCounter;
        private Dictionary<int, IEnumerable<MapBuilding>> dictBuildingsByHouseIndex;

        public HouseMap(Map map)
        {
            this.map = map;
            indexCounter = 0;
        }        

        public void DefineHouses()
        {
            houses = new List<House>();
            dictBuildingsByHouseIndex = new Dictionary<int, IEnumerable<MapBuilding>>();

            // Определение домов
            using (var scope = new Scope(map.Buildings, map))
            {
                scope.InitBuildingsContours();
                var buildingsWithoutHouses = scope.Buildings.ToList();// Копия списка
                // Пока есть не распределенные по домам здания
                while (buildingsWithoutHouses.Any())
                {
                    // Берем первое здание из списка нераспределенных зданий по домам
                    var firstBuilding = buildingsWithoutHouses[0];                    
                    buildingsWithoutHouses.RemoveAt(0);
                    // Если у здания еще нет дома, то определяем его
                    if (firstBuilding.House == null)
                    {
                        DefineHouseForBuilding(firstBuilding, scope.Buildings);
                    }
                }
            }
            // Окончательный список домов
            houses = dictBuildingsByHouseIndex.Select(s => s.Value.First().House).ToList();
        }

        /// <summary>
        /// Определение дома для здания
        /// </summary>
        /// <param name="building">Здание без дома</param>        
        /// <param name="allBuildings">Все здания</param>
        private void DefineHouseForBuilding(MapBuilding building, List<MapBuilding> allBuildings)
        {
            // Оффсет от контура здания наружу на 1м
            using (var offset = building.Contour.Offset(1, OffsetSide.Out).First())
            {
                var allNewHouseBuildings = new List<MapBuilding>();
                // Поиск ближайших зданий в области оффсета
                var nearestBuildings = map.GetBuildingsInExtents(offset.GeometricExtents);
                nearestBuildings.Remove(building);
                if (nearestBuildings.Any())
                {
                    var newHouseBuildings = new List<MapBuilding>();
                    newHouseBuildings.Add(building);
                    // Найти пересечение с ближайшими зданиями
                    foreach (var nearBuilding in nearestBuildings)
                    {
                        var ptsIntersect = new Point3dCollection();
                        offset.IntersectWith(nearBuilding.Contour, Intersect.OnBothOperands, ptsIntersect, IntPtr.Zero, IntPtr.Zero);
                        if (ptsIntersect.Count > 0)
                        {
                            newHouseBuildings.Add(nearBuilding);
                        }
                    }
                    // Найти все здания для нового дома                    
                    foreach (var newBuild in newHouseBuildings.GroupBy(g=>g.House))
                    {
                        if (newBuild.Key ==null)
                        {
                            allNewHouseBuildings.AddRange(newBuild);
                        }
                        else
                        {
                            allNewHouseBuildings.AddRange(dictBuildingsByHouseIndex[newBuild.Key.Index]);
                        }
                    }
                }
                else
                {
                    // Здание отдельно стоящее - дом
                    allNewHouseBuildings.Add(building);
                }
                NewHouse(allNewHouseBuildings);
            }
        }

        private void NewHouse(List<MapBuilding> buildings)
        {
            // Если среди зданий уже есть с домами, то удаление этих домов из словаря, т.к. будет создан новый общий дом
            var indexesHouseToRemove = buildings.Where(w => w.House != null).GroupBy(g => g.House.Index).Select(s => s.Key);
            foreach (var indexHouseOld in indexesHouseToRemove)
            {
                dictBuildingsByHouseIndex.Remove(indexHouseOld);
            }

            var house = new House(map.Doc, ++indexCounter);
            houses.Add(house);
            foreach (var item in buildings)
            {                
                item.House = house;
            }
            // Запись зданий в словарь по индексу дома
            dictBuildingsByHouseIndex[indexCounter] = buildings;            
        }

        public IEnumerator<House> GetEnumerator()
        {
            return houses.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return houses.GetEnumerator();
        }
    }
}
