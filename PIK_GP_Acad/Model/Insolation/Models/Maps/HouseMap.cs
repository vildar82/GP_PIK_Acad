using AcadLib.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Дома на карте
    /// </summary>
    public class HouseMap
    {
        private Map map;       

        public HouseMap(Map map)
        {
            this.map = map;
        }

        public void DefineHouses()
        {
            var houses = new List<House>();
            // Определение домов
            using (var scope = new Scope(map.Buildings, map))
            {
                var projectBuildings = scope.Buildings;//.Where(b => b.Building.IsProjectedBuilding);
                foreach (var building in projectBuildings)
                {
                    // Дом из блок-секций
                    if (!FindHouse(ref houses, building))
                    {
                        var house = new House(this);
                        house.Sections.Add(building);
                        houses.Add(house);
                    }
                }
                // Для каждого дома - создание общей полилинии
                int countHouse = 1;
                foreach (var house in houses)
                {
                    house.FrontGroup = this;
                    house.DefineContour();
                    // Заполнение оставшихся свойств дома
                    house.DefineName(countHouse);
                    countHouse++;
                }
            }
        }        

        private bool FindHouse(ref List<House> houses, MapBuilding building)
        {
            var findHouses = new List<House>();
            using (var offset = building.Contour.Offset(1, OffsetSide.Out).First())
            {
                foreach (var house in houses)
                {
                    foreach (var blInHouse in house.Sections)
                    {
                        using (var ptsIntersect = new Point3dCollection())
                        {
                            offset.IntersectWith(blInHouse.Contour, Intersect.OnBothOperands, ptsIntersect, IntPtr.Zero, IntPtr.Zero);
                            if (ptsIntersect.Count > 0)
                            {
                                findHouses.Add(house);
                                // Усреднение полилиний
                                foreach (var item in house.Sections)
                                {
                                    var contourItem = item.Contour;
                                    building.Contour.AverageVertexes(ref contourItem, toleranceVertex, true);
#if TEST
                                    //EntityHelper.AddEntityToCurrentSpace((Polyline)building.Contour.Clone());
                                    //EntityHelper.AddEntityToCurrentSpace((Polyline)contourItem.Clone());
#endif
                                }
                                break;
                            }
                        }
                    }
                }
            }
            if (findHouses.Any())
            {
                if (findHouses.Skip(1).Any())
                {
                    // Объединение нескольких домов в один общий
                    var bls = findHouses.SelectMany(s => s.Sections).ToList();
                    bls.Add(building);
                    var house = new House(this);
                    house.Sections = new ObservableCollection<MapBuilding>(bls);
                    houses.Add(house);
                    foreach (var h in findHouses)
                    {
                        houses.Remove(h);
                    }
                }
                else
                {
                    findHouses[0].Sections.Add(building);
                }
                return true;
            }
            return false;
        }
    }
}
