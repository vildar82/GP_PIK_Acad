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
                        Section section = new Section();
                        string errMsg = string.Empty;
                        // Площадь по внешней полилинии
                        Polyline plLayer;
                        var plContour = BlockSectionContours.FindContourPolyline(blRef, out plLayer);
                        section.AreaContour = plContour.Area;
                        // обработка атрибутов
                        parseAttrs(blRef.AttributeCollection, section, ref errMsg);
                        if (!string.IsNullOrEmpty(errMsg))
                        {
                            Inspector.AddError(errMsg, blRef, icon: System.Drawing.SystemIcons.Error);
                        }
                        sections.Add(section);
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

        private static void checkParam(Section section, ref string errMsg)
        {
            // Наименование
            if (string.IsNullOrEmpty(section.Name))
            {
                errMsg += "Наименование секции не определено.";
            }
        }

        private static void parseAttrs(AttributeCollection attrs, Section section, ref string errMsg)
        {
            if (attrs == null)
            {
                errMsg += "В блоке нет атрибутов.";
                return;
            }

            foreach (ObjectId idAtrRef in attrs)
            {
                var atrRef = idAtrRef.GetObject(OpenMode.ForRead, false, true) as AttributeReference;
                // Наименование
                if (string.Equals(atrRef.Tag, Settings.Default.AttrName, StringComparison.OrdinalIgnoreCase))
                {
                    section.SetName(atrRef.TextString);
                }
                // Площадь БКФН
                if (string.Equals(atrRef.Tag, Settings.Default.AttrAreaBKFN, StringComparison.OrdinalIgnoreCase))
                {
                    section.SetAreaBKFN(atrRef.TextString);
                }
                // Площадь квартир на одном этаже
                else if (string.Equals(atrRef.Tag, Settings.Default.AttrAreaApart, StringComparison.OrdinalIgnoreCase))
                {
                    section.SetAreaApart(atrRef.TextString);
                }
                // Площадь квартир общая на секцию (по всем этажам кроме 1)
                else if (string.Equals(atrRef.Tag, Settings.Default.AttrAreaApartTotal, StringComparison.OrdinalIgnoreCase))
                {
                    section.SetAreaApartTotal(atrRef.TextString);
                }
                // Кол этажей
                else if (string.Equals(atrRef.Tag, Settings.Default.AttrNumberFloor, StringComparison.OrdinalIgnoreCase))
                {
                    section.SetNumberFloor(atrRef.TextString);
                }
            }
            checkParam(section, ref errMsg);
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