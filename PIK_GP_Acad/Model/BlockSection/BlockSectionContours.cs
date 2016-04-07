using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.BlockSection;
using AcadLib;

namespace PIK_GP_Acad.BlockSection
{
    public static class BlockSectionContours
    {
        /// <summary>
        /// Построение полилиний контура у блоков Блок-Секций
        /// </summary>
        public static void CreateContour(Document doc)
        {
            using (var t = doc.TransactionManager.StartTransaction())
            {
                // Выбор блоков            
                SelectSection select = new SelectSection(doc);
                select.Select();
                if (select.IdsBlRefSections.Count == 0)
                    throw new Exception("Не найдены блоки Блок-Секций");
                else
                    doc.Editor.WriteMessage($"\nВыбрано {select.IdsBlRefSections.Count} блоков Блок-Секций.");

                int count = 0;

                AcadLib.Layers.LayerInfo layInfo = new AcadLib.Layers.LayerInfo("Defpoints");
                ObjectId layerIdPl = AcadLib.Layers.LayerExt.GetLayerOrCreateNew(layInfo);

                ObjectId msId = doc.Database.CurrentSpaceId;
                foreach (var idBlRefSec in select.IdsBlRefSections)
                {
                    var blRefSec = idBlRefSec.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                    try
                    {
                        var pl = getBtrContour(blRefSec);
                        if (pl != null)
                        {
                            var idPlCopy = CopyEnt(pl.Id, msId, doc.Database);
                            var plCopy = idPlCopy.GetObject(OpenMode.ForWrite, false, true) as Polyline;
                            plCopy.LayerId = layerIdPl;
                            plCopy.TransformBy(blRefSec.BlockTransform);
                            count++;
                        }
                    }
                    catch (Exception ex)
                    {
                        string blName = blRefSec.GetEffectiveName();
                        Inspector.AddError($"Ошибка построения контура для блока '{blName}' - {ex.Message}", blRefSec, System.Drawing.SystemIcons.Error);                        
                    }                    
                }
                doc.Editor.WriteMessage($"\nПостроено {count} полилиний контура блоков Блок-Секций.");
                t.Commit();
            }
        }

        private static Polyline getBtrContour(BlockReference blRefSec)
        {
            double area = 0;
            Polyline resVal = null;

            BlockTableRecord btrSec = blRefSec.BlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;            
            foreach (var idEnt in btrSec)
            {
                var pl = idEnt.GetObject(OpenMode.ForRead, false, true) as Polyline;
                if (pl == null || !pl.Visible) continue;
                if (pl.Area>area)
                {
                    resVal = pl;
                    area = pl.Area;
                }
            }
            return resVal;
        }

        private static ObjectId CopyEnt(ObjectId idEnt , ObjectId idBtrOwner, Database db)
        {
            IdMapping map = new IdMapping();
            ObjectIdCollection ids = new ObjectIdCollection(new[] { idEnt });
            db.DeepCloneObjects(ids, idBtrOwner, map, false);
            return map[idEnt].Value;
        }
    }
}
