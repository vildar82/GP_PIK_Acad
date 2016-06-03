using System;
using System.Collections.Generic;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.BlockSection
{
    public class ParserBlockSection
    {
        private List<ObjectId> _idsBlRefSections;
        private SectionService _service;

        public ParserBlockSection(SectionService sectionService, List<ObjectId> idsBlRefSections)
        {
            _service = sectionService;
            _idsBlRefSections = idsBlRefSections;
        }

        public List<Section> Sections { get; private set; }

        // Перебор блоков блок-секции и создание списка блок-секций
        public void Parse()
        {
            Sections = new List<Section>();
            foreach (var idBlRefSection in _idsBlRefSections)
            {
                Section section = new Section();
                string errMsg = string.Empty;
                var blRef = idBlRefSection.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                // Площадь по внешней полилинии
                var plContour = BlockSectionContours.FindContourPolyline(blRef);
                section.AreaContour = plContour.Area;
                // обработка атрибутов
                parseAttrs(blRef.AttributeCollection, section, ref errMsg);
                if (!string.IsNullOrEmpty(errMsg))
                {
                    Inspector.AddError(errMsg, blRef, icon: System.Drawing.SystemIcons.Error);
                }
                Sections.Add(section);
            }
        }

        private void checkParam(Section section, ref string errMsg)
        {
            // Наименование
            if (string.IsNullOrEmpty(section.Name))
            {
                errMsg += "Наименование секции не определено.";
            }
        }

        private void parseAttrs(AttributeCollection attrs, Section section, ref string errMsg)
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
    }
}