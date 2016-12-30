using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Проверка наложение зданий
    /// </summary>
    public static class CheckBuildingIntersect
    {
        private static Dictionary<ObjectId, HashSet<ObjectId>> dictChecked;        

        public static void Check()
        {
            var map = new Map();
            map.LoadMap();
            Check(map);
            map.Unsubscribe();
        }

        public static void Check(Map map)
        {            
            dictChecked = new Dictionary<ObjectId, HashSet<ObjectId>>();                
            using (var regions = new DisposableSet<Region>())
            using (var contours = new DisposableSet<Polyline>())
            {
                foreach (var build in map.Buildings)
                {
                    dictChecked.Add(build.Building.IdEnt, new HashSet<ObjectId>());
                    build.InitContour();
                    contours.Add(build.Contour);
                    var reg = BrepExtensions.CreateRegion(build.Contour);
                    build.Region = reg;
                    regions.Add(reg);
                }
                foreach (var build in map.Buildings)
                {
                    // здания в границах текущего здания
                    var nearest = map.GetBuildingsInExtents(build.ExtentsInModel);
                    if (!nearest.Any()) continue;
                    nearest.Remove(build);
                    // Проверка наложение с каждым ближайшим зданием
                    foreach (var nearBuild in nearest)
                    {
                        // Чтобы не проверять два взаимно пересекающихся дома 2 раза
                        if (dictChecked[build.Building.IdEnt].Contains(nearBuild.Building.IdEnt))
                            continue;
                        dictChecked[nearBuild.Building.IdEnt].Add(build.Building.IdEnt);

                        CheckIntersect(build, nearBuild);
                    }
                }
            }            
        }

        private static void CheckIntersect(MapBuilding build1, MapBuilding build2)
        {
            using (var r1 = (Region)build1.Region.Clone())
            using (var r2 = (Region)build2.Region.Clone())
            {
                r1.BooleanOperation(BooleanOperationType.BoolIntersect, r2);
                if (r1.NumChanges > 1 && r1.Area > 10)
                {                    
                    Inspector.AddError($"Наложение зданий. Площадь наложения {r1.Area.Round()}", 
                        r1.GeometricExtents, Matrix3d.Identity, System.Drawing.SystemIcons.Error);                    
                }
            }   
        }
    }
}
