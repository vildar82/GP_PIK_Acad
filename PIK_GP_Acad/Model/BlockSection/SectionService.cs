using System;
using System.Collections.Generic;
using AcadLib.Errors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.FCS;

namespace PIK_GP_Acad.BlockSection
{
    public class SectionService
    {
        public Document Doc { get; private set; }
        public Database Db { get; private set; }
        public Estimate Estimate { get; set; }      
        public DataSection DataSection { get; private set; }        
        public List<Section> Sections { get; private set; }
        public List<IClassificator> Classes { get; private set; }

        public SectionService (Document doc)
        {
            Doc = doc;
            Db = doc.Database;
        }

        // Подсчет секций
        public void CalcSections()
        {
            FCS.FCService.Init(Db);
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
                var selIds = select.Select(withRegions);
                if (selIds.Count == 0)
                {
                    throw new Exception("Не найдены блоки блок-секций");
                }
                else
                {
                    Doc.Editor.WriteMessage("\nВыбрано {0} блоков блок-секций.", selIds.Count);
                }
                Estimate = select.Estimate;

                // Обработка выбранных блоков           
                List<IClassificator> classes;
                Sections = ParserBlockSection.Parse(selIds, out classes);
                Classes = classes;

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