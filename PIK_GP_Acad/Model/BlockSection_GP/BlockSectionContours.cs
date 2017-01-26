using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.BlockSection_GP;
using AcadLib;

namespace PIK_GP_Acad.BlockSection_GP
{
    public static class BlockSectionContours
    {
        /// <summary>
        /// Построение полилиний контура у блоков Блок-Секций.
        /// Выбор блоков пользователем.
        /// </summary>
        public static void CreateContour(Document doc)
        {
            using (var t = doc.TransactionManager.StartTransaction())
            {
                // Выбор блоков            
                SelectSection select = new SelectSection(doc);
                var selIds = select.Select(false);
                if (selIds.Count == 0)
                    throw new Exception("Не найдены блоки Блок-Секций");
                else
                    doc.Editor.WriteMessage($"\nВыбрано {selIds.Count} блоков Блок-Секций.");

                int count = 0;

                AcadLib.Layers.LayerInfo layInfo = new AcadLib.Layers.LayerInfo("Defpoints");
                ObjectId layerIdPl = AcadLib.Layers.LayerExt.GetLayerOrCreateNew(layInfo);

                ObjectId msId = doc.Database.CurrentSpaceId;
                foreach (var idBlRefSec in selIds)
                {
                    var ent = idBlRefSec.GetObject(OpenMode.ForRead) as Entity;
                    if (ent is BlockReference)
                    {
                        var blRefSec = (BlockReference)ent;
                        string blName = blRefSec.GetEffectiveName();
                        if (SectionService.IsBlockNameSection(blName))
                        {
                            try
                            {
                                Polyline plLayer;
                                var pl = FindContourPolyline(blRefSec, out plLayer);
                                if (pl != null)
                                {
                                    var idPlCopy = pl.Id.CopyEnt(msId);
                                    var plCopy = idPlCopy.GetObject(OpenMode.ForWrite, false, true) as Polyline;
                                    plCopy.LayerId = layerIdPl;
                                    plCopy.TransformBy(blRefSec.BlockTransform);
                                    count++;
                                }
                            }
                            catch (Exception ex)
                            {                                
                                Inspector.AddError($"Ошибка построения контура для блока '{blName}' - {ex.Message}", blRefSec, System.Drawing.SystemIcons.Error);
                            }
                        }
                    }                  
                }
                doc.Editor.WriteMessage($"\nПостроено {count} полилиний контура блоков Блок-Секций.");
                t.Commit();
            }
        }

        /// <summary>
        /// Поиск контурной полилинии в блоке (по максимальной площаде)
        /// </summary>
        /// <param name="blRefSec">Блок</param>
        /// <param name="plLayer">Полилиния на слое outPlLayer</param>
        /// <param name="outPlLayer">Поиск полилинии на слое - out plLayer</param>        
        public static Polyline FindContourPolyline(BlockReference blRefSec, out Polyline plLayer, string outPlLayer = null)
        {
            double area = 0;
            Polyline resVal = null;
            plLayer = null;
            BlockTableRecord btrSec = blRefSec.BlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;            
            foreach (var idEnt in btrSec)
            {
                var pl = idEnt.GetObject(OpenMode.ForRead, false, true) as Polyline;
                if (pl == null || !pl.Visible) continue;
                if (pl.Layer.Equals (outPlLayer, StringComparison.OrdinalIgnoreCase))
                {
                    plLayer = pl;
                    continue;
                }
                if (pl.Area>area)
                {
                    resVal = pl;
                    area = pl.Area;
                }
            }
            return resVal;
        }        
    }
}
