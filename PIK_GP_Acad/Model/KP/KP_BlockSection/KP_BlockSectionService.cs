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

namespace PIK_GP_Acad.KP.KP_BlockSection
{
    public static class KP_BlockSectionService
    {
        public static Document Doc { get; private set; }
        public static Database Db { get; private set; }
        public static Editor Ed { get; private set; }

        public static void CreateTable()
        {
            Doc = Application.DocumentManager.MdiActiveDocument;
            Db = Doc.Database;
            Ed = Doc.Editor;
            // Выбор блоков блок-секций
            var blocks = selectBlocksection();

            // Подсчет блок-секций
            var dataSec = new DataSection(blocks);
            dataSec.Calc();

            // Создание таблицы и вставка
            var tableSec = new TableSection(dataSec);
            tableSec.Create();
        }

        private static List<BlockSection> selectBlocksection()
        {
            List<BlockSection> blocks = new List<BlockSection>();

            // Запрос выбора блоков
            var sel = Ed.SelectBlRefs("\nВыбор блоков блок-секций (Концепции):");

            using (var t = Db.TransactionManager.StartTransaction())
            {
                foreach (var idBlRef in sel)
                {
                    var blRef = idBlRef.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                    if (blRef == null) continue;
                    string blName = blRef.GetEffectiveName();
                    if(IsBlockSection(blName))
                    {
                        var blSec = new BlockSection(blRef);
                        blocks.Add(blSec);
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
    }
}
