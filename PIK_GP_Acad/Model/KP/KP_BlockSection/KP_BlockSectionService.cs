﻿using System;
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
        public const string blKpParkingLayerAxisContour ="UP_Секции_Оси";// "ГП_секции_посадка";
        public const string blKpParkingLayerAxisContourOld ="ГП_секции_посадка";
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

            // Перенос полилиний со старого слоя на новый 
            TransferLayerPlContours(Db);

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
                            var blSec = BlockSectionFactory.CreateBS(blRef, blName);
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

        public static void TransferLayerPlContours (Database db)
        {
            using (var t = db.TransactionManager.StartTransaction())
            {
                // Новый слой для контура ГНС внутри блок-секций - UP_Секции_ГНС
                var layerGNSInBS = AcadLib.Layers.LayerExt.CheckLayerState(blKpParkingLayerAxisContour);
                var bt = db.BlockTableId.GetObject( OpenMode.ForRead) as BlockTable;
                foreach (var btrId in bt)
                {
                    var btr = btrId.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    if (btr == null) continue;
                    if (IsBlockSection(btr.Name))
                    {
                        bool hasChanges = false;
                        foreach (var idEnt in btr)
                        {
                            var pl = idEnt.GetObject(OpenMode.ForRead, false, true) as Polyline;
                            if (pl == null) continue;
                            if (pl.Layer.Equals(blKpParkingLayerAxisContourOld, StringComparison.OrdinalIgnoreCase))
                            {
                                pl.UpgradeOpen();
                                pl.Layer = blKpParkingLayerAxisContour;
                                hasChanges = true;
                            }
                        }
                        if (hasChanges)
                        {
                            btr.UpdateAnonymousBlocks();
                        }
                    }
                }
                t.Commit();
            }
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

                    try
                    {
                        // Пересечение с другими секциями
                        var bsIntersects = rtreeBs.Intersects(bs.Rectangle);
                        if (bsIntersects.Count > 1)
                        {
                            List<Point2d> vertexModified = new List<Point2d> ();

                            foreach (var bsIntersect in bsIntersects)
                            {
                                // Пропускаем саму себя 
                                if (ReferenceEquals(bs, bsIntersect)) continue;

                                // Проверка пересечения полилиний контуров

                                // ГНС линия пересекаемой секции
                                var plExternItem =bsIntersect.PlExternalId.GetObject( OpenMode.ForRead, false, true) as Polyline;
                                plExternItem = plExternItem.Clone() as Polyline;
                                plExternItem.TransformBy(bsIntersect.Transform);

                                // Точки пересечения блок секций
                                Point3dCollection ptIntersects = new Point3dCollection ();
                                plExtern.IntersectWith(plExternItem, Intersect.OnBothOperands, new Plane(), ptIntersects, IntPtr.Zero, IntPtr.Zero);

                                ModifyPlExternal(plExtern, plExternItem, ptIntersects, bs);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Inspector.AddError($"Ошибка определения точного контура ГНС блок-секции - {ex}",
                            bs.IdBlRef, System.Drawing.SystemIcons.Warning);
                    }
                    bs.AreaGNS = plExtern.Area;

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

        private static void ModifyPlExternal (Polyline pl, Polyline plItem, Point3dCollection ptIntersects,
            BlockSection bs)
        {            
            if (ptIntersects.Count == 0) return;

            int numVertex = pl.NumberOfVertices;
            var modifiedPoints = new List<Point2d> ();            

            // для каждой точки пересечения найти ближайшую вершину на двух полилиниях
            foreach (Point3d ptIntersect in ptIntersects)
            {                
                var ptClosest = pl.GetClosestPointTo(ptIntersect, true);
                var param = pl.GetParameterAtPoint(ptClosest);
                var paramIndex = Convert.ToInt32(param);
                var ptVertexNearest = pl.GetPointAtParameter(paramIndex).Convert2d();
                int vertexIndex = paramIndex == numVertex? 0: paramIndex;

                if (modifiedPoints.Contains(ptVertexNearest))
                {
                    continue;
                }

                var ptClosestItem = plItem.GetClosestPointTo(ptIntersect, true);                
                var paramItem = plItem.GetParameterAtPoint(ptClosestItem);
                var paramIndexItem = Convert.ToInt32(paramItem);
                var ptVertexNearestItem = plItem.GetPointAtParameter(paramIndexItem).Convert2d();

                if ((ptVertexNearest - ptVertexNearestItem).Length>5)
                {
                    continue;
                }

                Point2d ptInsert =   ptVertexNearest.Center(ptVertexNearestItem);

                pl.RemoveVertexAt(vertexIndex);
                pl.AddVertexAt(vertexIndex, ptInsert, 0, 0, 0);

                modifiedPoints.Add(ptInsert);
            }            
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
