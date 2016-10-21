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
        public VisualTree (InsModel model)// : base (model.Doc)
        {
            Model = model;
        }

        public InsModel Model { get; set; }       

        public List<InsPoint> Points {
            get {
                var res = Model.Tree.Points?.ToList();
                if (res == null) return new List<InsPoint>();
                return res;
            }
        }

        public override List<Entity> CreateVisual ()
        {
            var points = Points;
            if (points == null || points.Count == 0 || points[0].Illums == null) return null;

            List<Entity> draws = new List<Entity>();
            List<List<Polyline>> plsAllTrees = new List<List<Polyline>>();
            List<List<ObjectId>> idsPlsAllTrees = new List<List<ObjectId>>();
            List<VisualOption> visOptions = new List<VisualOption>();
            foreach (var item in Model.Tree.TreeOptions.TreeVisualOptions)
            {
                plsAllTrees.Add(new List<Polyline>());
                idsPlsAllTrees.Add(new List<ObjectId>());
                var visOpt = new VisualOption(item.Color, Point3d.Origin, 60);
                visOptions.Add(visOpt);
            }

            var db = Model.Doc.Database;
            using (Model.Doc.LockDocument())
            using (var t = db.TransactionManager.StartTransaction())
            {
                var ms = SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject(OpenMode.ForWrite) as BlockTableRecord;
                // Получение полилиний елочек от всех точек (у каждой высоты визуализации - свой список полилиний)
                foreach (var item in Points)
                {
                    if (item.Illums == null) continue;
                    var plsItem = GetTreePolylines(item, visOptions);
                    for (int i = 0; i < plsItem.Count; i++)
                    {
                        var pl = plsItem[i];
                        ms.AppendEntity(pl);
                        t.AddNewlyCreatedDBObject(pl, true);
                        idsPlsAllTrees[i].Add(pl.Id);
                        pl.DowngradeOpen();
                    }
                }
                t.Commit();
            }
            using (Model.Doc.LockDocument())
            using (var t = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < idsPlsAllTrees.Count; i++)
                {                    
                    foreach (var item in idsPlsAllTrees[i])
                    {
                        var pl = item.GetObject(OpenMode.ForRead) as Polyline;
                        plsAllTrees[i].Add(pl);
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
                        var region = pls.Union((Region)overReg?.Clone());                        
                        var plsByLoop = region.GetPolylines();

                        if (overReg == null)
                        {
                            overReg = region;
                        }
                        else
                        {
                            overReg.BooleanOperation(BooleanOperationType.BoolUnite, region);
                        }
                        
                        var hatchs = GetHatchs(plsByLoop, visOpt);                        
                        draws.AddRange(hatchs);
                    }
                    catch { }
                    foreach (var item in pls)
                    {
                        item.UpgradeOpen();
                        item.Erase();
                    }
                }
                t.Commit();
            }        
            return draws;
        }        

        private List<Polyline> GetTreePolylines (InsPoint insPoint, List<VisualOption> visOptions)
        {
            var plsPointTrees = new List<Polyline>();

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
            var calcValues = Model.CalcService.CalcValues;

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

        private List<Hatch> GetHatchs (List<KeyValuePair<Polyline, BrepLoopType>> plsByLoop, VisualOption visOpt)
        {
            List<Hatch> resHatchs = new List<Hatch>();            
            foreach (var item in plsByLoop)
            {
                var h = item.Key.CreateHatch();
                SetEntityOpt(h, visOpt);
                resHatchs.Add(h);
                if (item.Value != BrepLoopType.LoopExterior)
                {
                    Logger.Log.Error("Внутренняя область в объединенном регионе елочек!!! Атас. Штриховка неправильно построится?!?!");
                }
            }
            return resHatchs;
        }
    }
}
