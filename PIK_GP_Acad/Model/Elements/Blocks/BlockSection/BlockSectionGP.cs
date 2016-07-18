using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Blocks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.BlockSection;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Elements.InfraworksExport;
using PIK_GP_Acad.OD;
using PIK_GP_Acad.OD.Records;

namespace PIK_GP_Acad.Elements.Blocks.BlockSection
{
    // Блок-секция
    public class BlockSectionGP : BlockSectionBase
    {
        public const string BlockNameMatch = "ГП_Блок-секция";        
        
        // Общая площадь квартир
        public double AreaApartTotal { get; private set; }
        // Площадь БКФН
        public double AreaBKFN { get; private set; }
        // Имя блок-секции
        public string Name { get; private set; } = string.Empty;                        

        public BlockSectionGP(BlockReference blRef, string blName) : base(blRef, blName)
        {          
            // обработка атрибутов
            parseAttrs();
            Define(blRef);
        }

        private void parseAttrs ()
        {
            // Наименование
            Name = GetPropValue<string>(SettingsBS.Default.AttrName);
            // Площадь БКФН
            AreaBKFN = GetPropValue<double>(SettingsBS.Default.AttrAreaBKFN);
            // Площадь квартир на одном этаже
            AreaLive = GetPropValue<double>(SettingsBS.Default.AttrAreaApart);
            // Площадь квартир общая на секцию (по всем этажам кроме 1)
            AreaApartTotal = GetPropValue<double>(SettingsBS.Default.AttrAreaApartTotal);
            // Кол этажей
            Floors = GetPropValue<int>(SettingsBS.Default.AttrNumberFloor);                                                       
        }
    }
}