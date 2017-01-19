using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using NetLib;

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
                    if (build.Contour == null)
                    {
                        Inspector.AddError($"Не определен контур здания - {NetLib.StringExt.ClearString(build.GetInfo())}, слой '{build.Building.Layer}'.", 
                            build.Building.IdEnt, System.Drawing.SystemIcons.Error);
                        continue;
                    }
                    contours.Add(build.Contour);
                    try
                    {
                        var reg = BrepExtensions.CreateRegion(build.Contour);
                        build.Region = reg;
                        regions.Add(reg);
                    }
                    catch(Exception ex)
                    {
                        Inspector.AddError($"Ошибка определения контура здания - '{NetLib.StringExt.ClearString(build.GetInfo())}', слой '{build.Building.Layer}'. {ex.Message}.", 
                            build.Building.IdEnt, System.Drawing.SystemIcons.Error);
                    }                    
                }
                foreach (var build in map.Buildings.Where(w=>w.Region != null))
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
                    Inspector.AddError($"Наложение зданий. Площадь наложения {NetLib.DoubleExt.Round(r1.Area,1)}. '{build1.GetInfo().Replace("\r\n", " ")}' и '{build2.GetInfo().Replace("\r\n", " ")}'.", 
                        r1.GeometricExtents, Matrix3d.Identity, System.Drawing.SystemIcons.Error);                    
                }
            }   
        }
    }
}
