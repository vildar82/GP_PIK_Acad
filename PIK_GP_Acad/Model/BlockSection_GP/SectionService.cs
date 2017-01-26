using System;
using System.Linq;
using System.Collections.Generic;
using AcadLib.Errors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using PIK_GP_Acad.Elements;
using PIK_GP_Acad.Elements.Blocks;
using PIK_GP_Acad.Elements.Blocks.BlockSection;
using PIK_GP_Acad.FCS;

namespace PIK_GP_Acad.BlockSection_GP
{
    public class SectionService
    {
        public Document Doc { get; private set; }
        public Database Db { get; private set; }
        public Estimate Estimate { get; set; }      
        public DataSection DataSection { get; private set; }        
        public List<BlockSectionGP> Sections { get; private set; }
        public List<IArea> Classes { get; private set; }

        public SectionService (Document doc)
        {
            Doc = doc;
            Db = doc.Database;
        }

        // Подсчет секций
        public void CalcSections()
        {
            FCService.Init(Db);
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
                List<IArea> classes;
                Sections = Parse(selIds, out classes, Doc.Editor);
                Classes = classes;

                // Подсчет площадей и типов блок-секций
                DataSection = new DataSection(this);
                DataSection.Calc();
                t.Commit();
            }
        }

        public static bool IsBlockNameSection (string name)
        {
            return name.StartsWith(BlockSectionGP.BlockSectionPrefix, StringComparison.OrdinalIgnoreCase);
        }

        public static List<BlockSectionGP> Parse (List<ObjectId> ids, out List<IArea> classes, Editor ed)
        {
            var classService = new ClassTypeService();
            classes = new List<IArea>();
            var sections = new List<BlockSectionGP>();

            var filteredBlocks = new List<string>();

            foreach (var idEnt in ids)
            {
                var ent = idEnt.GetObject(OpenMode.ForRead) as Entity;                

                if (ent is BlockReference)
                {
                    var section = ElementFactory.Create<BlockSectionGP>(ent);
                    if (section != null)
                    {
                        if (section.Error == null)
                        {
                            sections.Add(section);
                        }
                        else
                        {
                            Inspector.AddError(section.Error);
                        }
                    }
                    else
                    {
                        filteredBlocks.Add(((BlockReference)ent).GetEffectiveName());
                    }
                }
                else if (ent is Curve || ent is Hatch)
                {
                    var area = ElementFactory.Create<IArea>(ent, classService);                    
                    if (area != null)
                    {
                        classes.Add(area);
                    }
                }
            }

            if (filteredBlocks.Count>0)
            {
                ed.WriteMessage("\nОтфильтрованные блоки:");
                var groupsFilteredBlock = filteredBlocks.GroupBy(g=>g);
                foreach (var item in groupsFilteredBlock)
                {
                    ed.WriteMessage($"\n{item.Key} - {item.Count()} шт.");
                }
            }

            ed.WriteMessage($"\nОпределено блоков блок-секций ГП - {sections.Count}");

            return sections;
        }
    }
}