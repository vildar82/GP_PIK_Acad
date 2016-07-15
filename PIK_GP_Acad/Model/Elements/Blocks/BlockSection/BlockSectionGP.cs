using System;
using AcadLib.Blocks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.BlockSection;
using PIK_GP_Acad.Elements.Buildings;

namespace PIK_GP_Acad.Elements.Blocks.BlockSection
{
    // Блок-секция
    public class BlockSectionGP : BlockBase, IBuilding, IElement
    {
        public const string BlockNameMatch = "ГП_Блок-секция";

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
        public ObjectId IdPlContour { get; set; }

        public int Floors { get; set; }

        public Extents3d ExtentsInModel { get; set; }

        public Polyline ContourInModel {
            get {
                var pl = IdPlContour.GetObject(OpenMode.ForRead) as Polyline;
                pl.Clone();
                pl.TransformBy(Transform);
                return pl;
            }
        }

        public int Height { get; set; }

        public BlockSectionGP(BlockReference blRef, string blName) : base(blRef, blName)
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
            Name = GetPropValue<string>(SettingsBS.Default.AttrName);
            // Площадь БКФН
            AreaBKFN = GetPropValue<double>(SettingsBS.Default.AttrAreaBKFN);
            // Площадь квартир на одном этаже
            AreaApart = GetPropValue<double>(SettingsBS.Default.AttrAreaApart);
            // Площадь квартир общая на секцию (по всем этажам кроме 1)
            AreaApartTotal = GetPropValue<double>(SettingsBS.Default.AttrAreaApartTotal);
            // Кол этажей
            Floors = GetPropValue<int>(SettingsBS.Default.AttrNumberFloor);                                                       
        }        
    }
}