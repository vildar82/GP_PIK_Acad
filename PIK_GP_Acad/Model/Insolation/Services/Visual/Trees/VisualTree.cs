using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Models;
using AcadLib;
using AcadLib.Hatches;
using static PIK_GP_Acad.Insolation.Services.VisualHelper;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Визуализация клочек
    /// </summary>
    public class VisualTree : VisualTransient
    {
        public VisualTree (TreeModel model) //: base (model.Doc)
        {
            TreeModel = model;
        }

        public TreeModel TreeModel { get; set; }       

        public List<InsPoint> Points {
            get {
                var res = TreeModel.Points?.ToList();
                if (res == null) return new List<InsPoint>();
                return res;
            }
        }

        public override List<Entity> CreateVisual ()
        {
            var points = Points;
            if (points == null || points.Count == 0 || points.All(p => p.Illums == null)) return null;

            var draws = new List<Entity>();
            var plsAllTrees = new List<List<Polyline>>();
            var idsPlsAllTrees = new List<List<Polyline>>();
            var visOptions = new List<VisualOption>();
            foreach (var item in TreeModel.TreeOptions.TreeVisualOptions)
            {
                plsAllTrees.Add(new List<Polyline>());
                idsPlsAllTrees.Add(new List<Polyline>());
                var visOpt = new VisualOption(item.Color, Point3d.Origin, TreeModel.TreeOptions.Transparence);
                visOptions.Add(visOpt);
            }

            // Получение полилиний елочек от всех точек (у каждой высоты визуализации - свой список полилиний)
            foreach (var item in Points)
            {
                if (item.Illums == null) continue;
                var plsItem = GetTreePolylines(item, visOptions);
                for (int i = 0; i < plsItem.Count; i++)
                {
                    var pl = plsItem[i];
                    idsPlsAllTrees[i].Add(pl);
                }
            }
            for (int i = 0; i < idsPlsAllTrees.Count; i++)
            {
                foreach (var item in idsPlsAllTrees[i])
                {
                    plsAllTrees[i].Add(item);
                }
            }

            // Объединение полилиний по высотам
            Region overReg = null;
            for (int i = 0; i < plsAllTrees.Count; i++)
            {
                var visOpt = visOptions[i];
                var pls = plsAllTrees[i];
                try
                {
                    // Подставляемый регион для объединения
                    var regToUnion = (Region)overReg?.Clone();
                    var region = pls.Union(regToUnion);
                    regToUnion?.Dispose();


                    var hatch = region.CreateHatch();
                    if (hatch != null)
                    {
                        SetEntityOpt(hatch, visOpt);
                        draws.Add(hatch);
                    }                    

                    if (overReg == null)
                    {
                        overReg = region;
                    }
                    else
                    {
                        overReg.BooleanOperation(BooleanOperationType.BoolUnite, region);
                    }                    
                }
                catch { }
                foreach (var item in pls)
                {
                    item.Dispose();
                }
            }
            overReg?.Dispose();
            return draws;
        }        

        private List<Polyline> GetTreePolylines (InsPoint insPoint, List<VisualOption> visOptions)
        {
            var plsPointTrees = new List<Polyline>();

            if (insPoint.AngleStartOnPlane==0 && insPoint.AngleEndOnPlane==0)
            {
                return plsPointTrees;
            }

            Point2d p1 = insPoint.Point.Convert2d();
            Point2d p2= p1;
            Point2d p3;
            Point2d p4;

            var treeVisOptions = insPoint.Model.Tree.TreeOptions.TreeVisualOptions;            
            for (int i = 0; i < treeVisOptions.Count; i++)
            {
                var pl = GetPolylineTreeOption(insPoint, treeVisOptions[i], visOptions[i], p1, p2, out p3, out p4);
                p1 = p4;
                p2 = p3;
                plsPointTrees.Add(pl);
            }
            return plsPointTrees;
        }

        /// <summary>
        /// Графика визуализации заданной высотности (настройка визуализации елочек) 
        /// </summary>
        /// <param name="insPoint">инс точка</param>
        /// <param name="treeOpt">Настройка визуализации высоты улочек</param>
        /// <param name="p1">Первая точка елочки (левый верхний угол прямоугольника). Для первой елочки p1=p2=insPoint</param>
        /// <param name="p2">Правый верхний угол елочки</param>
        /// <param name="p3">Нижний правый угол елочки (возвращается)</param>
        /// <param name="p4">Нижений левый угол елочки (возвращается)</param>
        /// <returns></returns>
        private Polyline GetPolylineTreeOption (InsPoint insPoint, TreeVisualOption treeOpt, VisualOption visOpt,
            Point2d p1, Point2d p2, out Point2d p3, out Point2d p4)
        {
            var ptOrig = insPoint.Point.Convert2d();
            var calcValues = TreeModel.Model.CalcService.CalcValues;

            double cShadow;
            // Высота тени (на заданной настройкой высоте елочки) - катет по Y
            var yShadow = calcValues.YShadowLineByHeight(treeOpt.Height, out cShadow);

            // Луч падения - катет по X до точки на луче, на заданой высоте
            var xRay = calcValues.GetXRay(yShadow, insPoint.AngleStartOnPlane);
            p3 = ptOrig + new Vector2d(xRay, -yShadow);

            xRay = calcValues.GetXRay(yShadow, insPoint.AngleEndOnPlane);
            p4 = ptOrig + new Vector2d(xRay, -yShadow);
            
            var points = new List<Point2d> { p1, p2, p3, p4 };

            var pl = CreatePolyline(points, visOpt);
            return pl;
        }

        //private Hatch GetHatch (Region region)
        //{
        //    var plsByLoop = region.GetPoints2dByLoopType();
        //    var externalLoops = plsByLoop.Where(p => p.Value != BrepLoopType.LoopInterior).ToList();
        //    var interiorLoops = plsByLoop.Where(p => p.Value == BrepLoopType.LoopInterior).ToList();

        //    if (!externalLoops.Any())
        //    {
        //        return null;
        //    }

        //    var h = new Hatch();
        //    h.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");            

        //    foreach (var item in externalLoops)
        //    {
        //        var pts2dCol = item.Key;
        //        pts2dCol.Add(item.Key[0]);
        //        h.AppendLoop(HatchLoopTypes.External, pts2dCol, new DoubleCollection(new double[externalLoops.Count+1]));
        //    }

        //    if (interiorLoops.Any())
        //    {
        //        foreach (var item in interiorLoops)
        //        {
        //            var pts2dCol = item.Key;
        //            pts2dCol.Add(item.Key[0]);
        //            h.AppendLoop(HatchLoopTypes.SelfIntersecting, pts2dCol, new DoubleCollection(new double[interiorLoops.Count + 1]));
        //        }
        //    }

        //    h.EvaluateHatch(true);                                
        //    return h;
        //}        
    }
}
