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

        public static void Insert(string blName, Database db, List<Property> props = null)
        {
            // Выбор и вставка блока     
            LoadBlock(new List<string> { blName }, db);            
            BlockInsert.Insert(blName, null, props);
        }

        public static void LoadBlock(List<string> blNames, Database db)
        {
            Block.CopyBlockFromExternalDrawing(blNames, fileBlocks, db, DuplicateRecordCloning.Ignore);
        }
    }
}
