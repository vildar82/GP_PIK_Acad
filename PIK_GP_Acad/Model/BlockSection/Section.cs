using System;
using AcadLib.Blocks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.BlockSection
{
    // Блок-секция
    public class Section : BlockBase
    {
        // Площадь полилинии контура
        public double AreaContour { get; set; }
        // Площадь квартир
        public double AreaApart { get; private set; }

        // Общая площадь квартир
        public double AreaApartTotal { get; private set; }

        // Площадь БКФН
        public double AreaBKFN { get; private set; }

        // Имя блок-секции
        public string Name { get; private set; } = string.Empty;

        // Кол этажей
        public int NumberFloor { get; private set; }
        public ObjectId IdPlContour { get; set; }

        public Section(BlockReference blRef, string blName) : base(blRef, blName)
        {            
            // Площадь по внешней полилинии
            Polyline plLayer;
            var plContour = BlockSectionContours.FindContourPolyline(blRef, out plLayer);
            IdPlContour = plContour.Id;
            AreaContour = plContour.Area;
            // обработка атрибутов
            parseAttrs();            
        }

        private void parseAttrs ()
        {
            // Наименование
            Name = GetPropValue<string>(Settings.Default.AttrName);
            // Площадь БКФН
            AreaBKFN = GetPropValue<double>(Settings.Default.AttrAreaBKFN);
            // Площадь квартир на одном этаже
            AreaApart = GetPropValue<double>(Settings.Default.AttrAreaApart);
            // Площадь квартир общая на секцию (по всем этажам кроме 1)
            AreaApartTotal = GetPropValue<double>(Settings.Default.AttrAreaApartTotal);
            // Кол этажей
            NumberFloor = GetPropValue<int>(Settings.Default.AttrNumberFloor);                                                       
        }        
    }
}