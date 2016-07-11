﻿using System;
using System.Collections.Generic;
using AcadLib.Errors;
using Autodesk.AutoCAD.ApplicationServices;

namespace PIK_GP_Acad.BlockSection
{
    public class SectionService
    {
        public Estimate Estimate { get; set; }

        public SectionService(Document doc)
        {
            Doc = doc;
        }

        public DataSection DataSection { get; private set; }
        public Document Doc { get; private set; }
        public List<Section> Sections { get; private set; }

        // Подсчет секций
        public void CalcSections()
        {
            GetData(true);
            // Построение таблицы
            TableSecton tableSection = new TableSecton(this);
            tableSection.CreateTable();
        }        

        public void CalcSectionsForKP()
        {
            GetData(false);
            // Построение таблицы
            TableSectonKP tableSection = new TableSectonKP(this);
            tableSection.CreateTable();
        }

        private void GetData(bool withRegions)
        {
            using (var t = Doc.TransactionManager.StartTransaction())
            {
                // Выбор блоков
                SelectSection select = new SelectSection(Doc);
                select.Select(withRegions);
                if (select.IdsBlRefSections.Count == 0)
                {
                    throw new Exception("Не найдены блоки блок-секций");
                }
                else
                {
                    Doc.Editor.WriteMessage("\nВыбрано {0} блоков блок-секций.", select.IdsBlRefSections.Count);
                }
                Estimate = select.Estimate;

                // Обработка выбранных блоков
                ParserBlockSection parser = new ParserBlockSection(this, select.IdsBlRefSections);
                parser.Parse();
                Sections = parser.Sections;

                // Подсчет площадей и типов блок-секций
                DataSection = new DataSection(this);
                DataSection.Calc();
                t.Commit();
            }
        }

        public static bool IsBlockNameSection (string name)
        {
            return name.StartsWith(Settings.Default.BlockSectionPrefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}