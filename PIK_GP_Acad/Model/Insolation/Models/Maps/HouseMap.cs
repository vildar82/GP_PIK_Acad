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
using AcadLib.Errors;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Дома на карте
    /// </summary>
    public class HouseMap : IEnumerable<House>, IDisposable
    {
        private static Tolerance toleranceVertex = new Tolerance(1, 1);
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
                scope.InitBuildingContours();
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
                        DefineHouseForBuilding(firstBuilding);
                    }
                }
                // Окончательный список домов
                houses = new List<House>();
                foreach (var item in dictBuildingsByHouseIndex)
                {
                    var house = item.Value.First().House;
                    houses.Add(house);
                    house.Sections = new System.Collections.ObjectModel.ObservableCollection<MapBuilding>(item.Value);
                    // Определение контуров домов     
                    try
                    {
                        house.DefineContour();
                    }
                    catch(Exception ex)
                    {
                        Inspector.AddError($"Ошибка определения контура дома - {ex.Message}", 
                            house.GetExtents(), Matrix3d.Identity, System.Drawing.SystemIcons.Error);
                        Logger.Log.Error(ex, "HouseMap.DefineHouses()");
                    }
                }
            }            
        }

        /// <summary>
        /// Дома в указанной области
        /// </summary>
        /// <param name="ext">Область на чертеже</param>        
        public List<House> GetHousesInExtents(Extents3d ext)
        {
            return map.GetBuildingsInExtents(ext).GroupBy(g=>g.House).Select(s=>s.Key).ToList();
        }

        /// <summary>
        /// Определение дома для здания
        /// </summary>
        /// <param name="building">Здание без дома</param>                
        private void DefineHouseForBuilding(MapBuilding building)
        {
            var allIntersectBuildings = new HashSet<MapBuilding>();
            GetIntersectBuildings(building, ref allIntersectBuildings);
            // Найти все связанные здания для нового дома                    
            var allNewHouseBuildings = GetAllReferencedBuildings(allIntersectBuildings);
            NewHouse(allNewHouseBuildings);
        }

        /// <summary>
        /// Пересекающиеся дома
        /// </summary>        
        private void GetIntersectBuildings(MapBuilding building, ref HashSet<MapBuilding> intersectBuildings)
        {            
            using (var offset = building.Contour.Offset(1, OffsetSide.Out).First())
            {
                // Ближайшие дома без уже найденных зданий
                intersectBuildings.Add(building);
                var nearestBuildings = map.GetBuildingsInExtents(offset.GeometricExtents).Except(intersectBuildings).
                    Where(w=>w.Building.IsProjectedBuilding == building.Building.IsProjectedBuilding).ToList();                
                var curIntersectBuildings = new List<MapBuilding>();
                foreach (var nearBuilding in nearestBuildings)
                {
                    var ptsIntersect = new Point3dCollection();
                    offset.IntersectWith(nearBuilding.Contour, Intersect.OnBothOperands, ptsIntersect, IntPtr.Zero, IntPtr.Zero);
                    if (ptsIntersect.Count > 0)
                    {
                        if (intersectBuildings.Add(nearBuilding) && nearBuilding.House == null)
                        {
                            curIntersectBuildings.Add(nearBuilding);
                            // Усреднение вершин двух секций
                            var contourItem = nearBuilding.Contour;
                            try
                            {
                                building.Contour.AverageVertexes(ref contourItem, toleranceVertex, true);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                foreach (var intersectBuild in curIntersectBuildings)
                {
                    GetIntersectBuildings(intersectBuild, ref intersectBuildings);
                }
            }
        }

        /// <summary>
        /// Все связанные здания (через их дома)
        /// </summary>
        /// <param name="buildings">Здания</param>
        /// <returns>Все здания связанные с заданными зданиями</returns>
        private List<MapBuilding> GetAllReferencedBuildings(IEnumerable<MapBuilding> buildings)
        {
            var allNewHouseBuildings = new List<MapBuilding>();
            foreach (var newBuild in buildings.GroupBy(g => g.House))
            {
                if (newBuild.Key == null)
                    allNewHouseBuildings.AddRange(newBuild);
                else
                    allNewHouseBuildings.AddRange(dictBuildingsByHouseIndex[newBuild.Key.Index]);
            }
            return allNewHouseBuildings;
        }

        /// <summary>
        /// Определение связанных объектов для всез домов
        /// </summary>
        public void DefineConnections()
        {
            if (houses == null) return;
            foreach (var house in houses)
            {
                house.DefineConnectionDbSel();
            }
        }

        /// <summary>
        /// Очистка связывания домов с объеектами базы
        /// </summary>
        public void ClearDbConnections()
        {
            if (houses == null) return;
            foreach (var item in houses.Where(h=>h.SelectedHouseDb != null))
            {
                item.SelectedHouseDb = null;
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

            var house = new House(map.Model, ++indexCounter);
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

        public void Dispose()
        {
            if (houses == null) return;
            foreach (var item in houses)
            {
                item.Dispose();
            }
        }
    }
}
