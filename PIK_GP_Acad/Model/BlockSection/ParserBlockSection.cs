using System;
using System.Linq;
using System.Collections.Generic;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.FCS;

namespace PIK_GP_Acad.BlockSection
{
    public static class ParserBlockSection
    {      
        // Перебор блоков блок-секции и создание списка блок-секций
        public static List<Section> Parse (List<ObjectId> ids, out List<IClassificator> classes)
        {
            ClassTypeService classService = new ClassTypeService();
            classes = new List<IClassificator>();
           var sections = new List<Section>();
            foreach (var idEnt in ids)
            {
                var ent = idEnt.GetObject(OpenMode.ForRead) as Entity;

                if (ent is BlockReference)
                {
                    var blRef = (BlockReference)ent;
                    string blName = blRef.GetEffectiveName();
                    if (SectionService.IsBlockNameSection(blName))
                    {
                        Section section = new Section(blRef, blName);
                        if (section.Error == null)
                        {
                            sections.Add(section);
                        }
                        else
                        {
                            Inspector.AddError(section.Error);
                        }
                    }
                }
                else if (ent is Curve || ent is Hatch)
                {
                    KeyValuePair<string, List<FCProperty>> tag;
                    if (FCService.GetTag(ent.Id, out tag))
                    {
                        var classType = classService.GetClassType(tag.Key);
                        if (classType != null)
                        {
                            var value = GetValue(idEnt, classType.UnitFactor, classType.ClassName);
                            if (value != 0)
                            {
                                Classificator c = new Classificator(idEnt, classType, value);
                                classes.Add(c);
                            }
                        }
                    }
                }
            }
            return sections;
        }

        

        private static double GetValue (ObjectId idEnt, double unitFactor, string tag)
        {
            double res = 0;
            var ent = idEnt.GetObject(OpenMode.ForRead, false, true);

            if (ent is Curve)
            {
                var curve = ent as Curve;
                res = curve.Area * unitFactor;
            }
            else if (ent is Hatch)
            {
                var h = ent as Hatch;
                res = h.Area * unitFactor;
            }
            else
            {
                Inspector.AddError($"Неподдерживаемый тип объекта - {idEnt.ObjectClass.Name}. Классификатор - {tag}",
                        idEnt, System.Drawing.SystemIcons.Error);
            }
            if (res == 0)
            {
                Inspector.AddError($"Не определена площадь объекта", idEnt, System.Drawing.SystemIcons.Error);
            }

            return res;
        }
    }
}