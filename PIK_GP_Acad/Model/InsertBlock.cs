using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad
{
    public static class InsertBlock
    {
        // Файл шаблонов блоков
        static string fileBlocks = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.LocalSettingsFolder, @"Blocks\ГП\ГП_Блоки.dwg");

        public static void Insert(string blName, Document doc, List<Property> props = null)
        {            
            // Выбор и вставка блока                 
            Block.CopyBlockFromExternalDrawing(blName, fileBlocks, doc.Database, DuplicateRecordCloning.Ignore);
            BlockInsert.Insert(blName, null, props);
        }
    }
}
