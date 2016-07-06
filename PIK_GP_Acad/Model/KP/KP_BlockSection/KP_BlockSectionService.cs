using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib;
using Autodesk.AutoCAD.EditorInput;
using System.Text.RegularExpressions;
using AcadLib.Errors;
using AcadLib.RTree.SpatialIndex;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;

namespace PIK_GP_Acad.KP.KP_BlockSection
{
    public static class KP_BlockSectionService
    {
        /// <summary>
        /// Слой контура в блоке блок-секции
        /// </summary>
        public const string blKpParkingLayerContour = "ГП_секции_посадка";
        public static Document Doc { get; private set; }
        public static Database Db { get; private set; }
        public static Editor Ed { get; private set; }        

        /// <summary>
        /// Закраска контуров блок-секций штриховкой (заливкой)
        /// </summary>
        public static void Fill ()
        {
            Doc = Application.DocumentManager.MdiActiveDocument;
            Db = Doc.Database;
            Ed = Doc.Editor;

            // выбор блок-секций
            var blocks = SelectBlocksection(true, true);
            // Построение контуроов ГНС с заливкой
            DefineHouses(blocks, true);
        }

        public static void CreateTable(bool isNew)
        {
            Doc = Application.DocumentManager.MdiActiveDocument;
            Db = Doc.Database;
            Ed = Doc.Editor;

            Options.PromptOptions();

            // Выбор блоков блок-секций
            var blocks = SelectBlocksection(isNew, false);

            // Определение точных контуров ГНС - с учетом стыковки блок-секций
            if (isNew)
            {
                try
                {
                    DefineHouses(blocks, false);
                }
                catch (Exception ex)
                {
                    Inspector.AddError($"Ошибка определения точного контура ГНС Блок-Секций - {ex}");
                }                
            }

            // Подсчет блок-секций
            var dataSec = new DataSection(blocks, Options.Instance);
            dataSec.Calc();

            // Создание таблицы и вставка
            var tableSec = new TableSection(dataSec);
            tableSec.Create();
        }        

        private static List<BlockSection> SelectBlocksection(bool isNew, bool fill)
        {
            List<BlockSection> blocks = new List<BlockSection>();

            // Запрос выбора блоков
            var sel = Ed.Select("\nВыбор блоков блок-секций (Концепции):");

            using (var t = Db.TransactionManager.StartTransaction())
            {
                foreach (var idBlRef in sel)
                {
                    if (isNew)
                    {
                        var plGns = idBlRef.GetObject( OpenMode.ForRead, false, true) as Polyline;
                        if (plGns != null)
                        {
                            if (plGns.Layer.Equals(Options.Instance.LayerBSContourGNS, StringComparison.OrdinalIgnoreCase))
                            {
                                plGns.UpgradeOpen();
                                plGns.Erase();                                
                            }
                            continue;
                        }                        
                    }
                    if (fill)
                    {
                        var plHatch = idBlRef.GetObject( OpenMode.ForRead, false, true) as Hatch;
                        if (plHatch != null)
                        {
                            if (plHatch.Layer.Equals(Options.Instance.LayerBSContourGNS, StringComparison.OrdinalIgnoreCase))
                            {
                                plHatch.UpgradeOpen();
                                plHatch.Erase();
                            }
                            continue;
                        }
                    }

                    var blRef = idBlRef.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                    if (blRef == null) continue;
                    string blName = blRef.GetEffectiveName();
                    if(IsBlockSection(blName))
                    {
                        try
                        {
                            var blSec = new BlockSection(blRef, blName);
                            if (blSec.Error != null)
                            {
                                Inspector.AddError(blSec.Error);
                            }
                            else
                            {
                                blocks.Add(blSec);
                            }                            
                        }
                        catch (Exception ex)
                        {
                            Inspector.AddError(ex.Message, blRef, System.Drawing.SystemIcons.Error);
                        }                        
                    }
                }
                t.Commit();
            }
            return blocks;
        }

        public static bool IsBlockSection(string blName)
        {
            return Regex.IsMatch(blName, Options.Instance.BlockSectionNameMatch);
        }

        /// <summary>
        /// Определение точных габаритов Блок-Секций - по стыковке Блок-Секций
        /// </summary>        
        private static void DefineHouses (List<BlockSection> blocks, bool fill)
        {
            // Дерево блок-секций
            var rtreeBs = GetRtreeBs(blocks);
            Tolerance tolerance = new Tolerance (0.1, 0.1);

            using (var t = Db.TransactionManager.StartTransaction())
            {
                var layerGNS = AcadLib.Layers.LayerExt.CheckLayerState(Options.Instance.LayerBSContourGNS);

                var cs = Db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                foreach (var bs in blocks)
                {
                    // Полилиния ГНС этой секции скопированная в модель
                    var idPlExtern = bs.PlExternalId.CopyEnt(cs.Id);
                    var plExtern = idPlExtern.GetObject(OpenMode.ForWrite, false, true) as Polyline;
                    plExtern.Layer = Options.Instance.LayerBSContourGNS;
                    plExtern.ColorIndex = 256;
                    plExtern.Linetype = SymbolUtilityServices.LinetypeByLayerName;
                    plExtern.LineWeight = LineWeight.ByLayer;
                    plExtern.TransformBy(bs.Transform);

                    if (fill)
                    {
                        // Заливка контура штриховкой
                        try
                        {
                            FillContour(bs, plExtern, cs, t);
                        }
                        catch (Exception ex)
                        {
                            Inspector.AddError($"Ошибка заполнения штриховки контура ГНС блок-секции - {ex}",
                                bs.IdBlRef, System.Drawing.SystemIcons.Warning);
                        }
                    }

                    try
                    {
                        // Пересечение с другими секциями
                        var bsIntersects = rtreeBs.Intersects(bs.Rectangle);
                        if (bsIntersects.Count > 1)
                        {
                            // Осевая полилиния этой секции
                            var plAxis =bs.PlAxisId.GetObject( OpenMode.ForRead, false, true) as Polyline;
                            plAxis = plAxis.Clone() as Polyline;
                            plAxis.ColorIndex = 1;
                            plAxis.TransformBy(bs.Transform);

                            List<Point2d> vertexModified = new List<Point2d> ();

                            foreach (var bsIntersect in bsIntersects)
                            {
                                // Пропускаем саму себя 
                                if (ReferenceEquals(bs, bsIntersect)) continue;

                                // Проверка пересечения полилиний контуров

                                // Осевая линия пересекаемой секции
                                var plAxisItem =bsIntersect.PlAxisId.GetObject( OpenMode.ForRead, false, true) as Polyline;
                                plAxisItem = plAxisItem.Clone() as Polyline;
                                plAxisItem.TransformBy(bsIntersect.Transform);

                                // Точки пересечения блок секций
                                Point3dCollection ptIntersects = new Point3dCollection ();
                                plAxis.IntersectWith(plAxisItem, Intersect.OnBothOperands, new Plane(), ptIntersects, IntPtr.Zero, IntPtr.Zero);

                                ModifyPlExternal(plExtern, ptIntersects, tolerance, ref vertexModified, bs);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Inspector.AddError($"Ошибка определения точного контура ГНС блок-секции - {ex}",
                            bs.IdBlRef, System.Drawing.SystemIcons.Warning);
                    }
                    bs.AreaByExternalWalls = plExtern.Area;           
                }
                t.Commit();
            }
        }

        

        private static RTree<BlockSection> GetRtreeBs (List<BlockSection> blocks)
        {
            RTree<BlockSection> rtree = new RTree<BlockSection>  ();
            foreach (var item in blocks)
            {
                Rectangle r = item.Rectangle;
                rtree.Add(r, item);
            }
            return rtree;
        }

        private static void ModifyPlExternal (Polyline pl, Point3dCollection ptIntersects, Tolerance tolerance,
           ref List<Point2d> vertexModified, BlockSection bs)
        {            
            if (ptIntersects.Count == 0) return;
            if (ptIntersects.Count > 2)
            {
                Extents3d extIntersect = new Extents3d ();
                foreach (Point3d item in ptIntersects)
                {
                    extIntersect.AddPoint(item);
                }
                Inspector.AddError($"Больше двух точек пересечения осевых контуров блок-секций", 
                    extIntersect, bs.IdBlRef, System.Drawing.SystemIcons.Warning);
                return;
            }
            int numVertex = pl.NumberOfVertices;
            var ptIntersect1 =ptIntersects[0];
            var ptIntersect2 =ptIntersects[1];
            var ptClosest1 = pl.GetClosestPointTo(ptIntersect1, true);
            var ptClosest2 = pl.GetClosestPointTo(ptIntersect2, true);
            var param1 = pl.GetParameterAtPoint(ptClosest1);
            var param2 = pl.GetParameterAtPoint(ptClosest2);
            var paramIndex1 = Convert.ToInt32(param1);
            var paramIndex2 = Convert.ToInt32(param2);
            var ptVertexNearest1 = pl.GetPointAtParameter(paramIndex1).Convert2d();
            var ptVertexNearest2 = pl.GetPointAtParameter(paramIndex2).Convert2d();    
            
            if (vertexModified.Contains(ptVertexNearest1) ||
                vertexModified.Contains(ptVertexNearest2))
            {
                Extents3d extIntersect = new Extents3d ();               
                extIntersect.AddPoint(ptVertexNearest1.Convert3d());
                extIntersect.AddPoint(ptVertexNearest2.Convert3d());

                // Эти точки контура блок-секции уже изменялись - недопустимая ситуация
                Inspector.AddError($"Повторное изменение точек внешнего контура блок-секции. Ошибка.",
                    extIntersect, bs.IdBlRef, System.Drawing.SystemIcons.Warning);
                return;
            }

            LineSegment2d seg1;
            LineSegment2d seg2;
            int vertexIndex1 = paramIndex1 == numVertex? 0: paramIndex1;
            int vertexIndex2 = paramIndex2 == numVertex? 0: paramIndex2;
            int segIndex1 =vertexIndex1;
            int segIndex2 =vertexIndex2;
            if (paramIndex2 - paramIndex1 == 1)
            {
                segIndex1 = paramIndex1 == 0 ? numVertex - 1 : paramIndex1 - 1;                
            }
            else if (paramIndex1 - paramIndex2 == 1)
            {
                segIndex2 = paramIndex2 == 0 ? numVertex - 1 : paramIndex2 - 1;                
            }
            else
            {
                if (paramIndex1 > paramIndex2)
                {
                    segIndex1 = paramIndex1 - 1;                    
                }
                else
                {
                    segIndex2 = paramIndex2 - 1;
                }
            }

            seg1 = pl.GetLineSegment2dAt(segIndex1);
            seg2 = pl.GetLineSegment2dAt(segIndex2);

            var ptInsert1 = seg1.GetNormalPoint(ptIntersect1.Convert2d()).Point;
            var ptInsert2 = seg2.GetNormalPoint(ptIntersect2.Convert2d()).Point;

            pl.RemoveVertexAt(vertexIndex1);
            pl.AddVertexAt(vertexIndex1, ptInsert1, 0, 0, 0);
            pl.RemoveVertexAt(vertexIndex2);
            pl.AddVertexAt(vertexIndex2, ptInsert2, 0, 0, 0);

            vertexModified.Add(ptInsert1);
            vertexModified.Add(ptInsert1);
        }

        private static void FillContour (BlockSection bs, Polyline plExtern, BlockTableRecord cs, Transaction t)
        {
            var h = new Hatch();
            h.SetDatabaseDefaults();
            h.Layer = Options.Instance.LayerBSContourGNS;
            h.LineWeight = LineWeight.LineWeight015;
            h.Linetype = SymbolUtilityServices.LinetypeContinuousName;
            h.Color = GetFillColor(bs); // Color.FromRgb(250, 250, 250);
            //h.Transparency = new Transparency(80);
            h.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");            
            cs.AppendEntity(h);            
            t.AddNewlyCreatedDBObject(h, true);
            h.Associative = true;
            h.HatchStyle = HatchStyle.Normal;                        

            // добавление контура полилинии в гштриховку
            var ids = new ObjectIdCollection();
            ids.Add(plExtern.Id);
            h.AppendLoop(HatchLoopTypes.Default, ids);
            h.EvaluateHatch(true);
            
            var orders = cs.DrawOrderTableId.GetObject(OpenMode.ForWrite) as DrawOrderTable;
            orders.MoveToBottom(new ObjectIdCollection(new[] { h.Id }));
        }

        private static Color GetFillColor (BlockSection bs)
        {
            if (bs.Floors <= 15)
                return Color.FromRgb(255, 255, 255);
            else if (bs.Floors > 20)
                return Color.FromColorIndex(ColorMethod.ByAci, 253);
            else            
                return Color.FromColorIndex(ColorMethod.ByAci, 254);
            
            //byte r = Convert.ToByte(255 - bs.Floors*5);
            //byte g = Convert.ToByte(255 - bs.Floors*5);
            //byte b = Convert.ToByte(255 - bs.Floors*5);

        }
    }
}
