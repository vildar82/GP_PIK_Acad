using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad
{
    public static class InsertBlock
    {
        // Файл шаблонов блоков
        static string fileBlocks = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.LocalSettingsFolder, @"Blocks\ГП\ГП_Блоки.dwg");

        public static void Insert(string blName, Document doc)
        {            
            // Выбор и вставка блока                 
            AcadLib.Blocks.Block.CopyBlockFromExternalDrawing(blName, fileBlocks, doc.Database, DuplicateRecordCloning.Ignore);
            AcadLib.Blocks.BlockInsert.Insert(blName);
        }
    }
}
