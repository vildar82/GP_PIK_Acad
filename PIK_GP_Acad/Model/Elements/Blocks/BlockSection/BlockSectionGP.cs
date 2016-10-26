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

        private List<string> ReductionFactorIgnoringNamesBS = new List<string> { "Б-13", "Б-14" };
        
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
            // Кол этажей
            Floors = GetPropValue<int>(SettingsBS.Default.AttrNumberFloor);
            // Наименование
            Name = GetPropValue<string>(SettingsBS.Default.AttrName);
            // Площадь БКФН
            AreaBKFN = GetPropValue<double>(SettingsBS.Default.AttrAreaBKFN);
            //if (Floors >= 30)
            //    AreaBKFN *= 0.92;
            // Площадь квартир на одном этаже
            AreaLive = GetPropValue<double>(SettingsBS.Default.AttrAreaApart);
            // Площадь квартир общая на секцию (по всем этажам кроме 1)
            //AreaApartTotal = GetPropValue<double>(SettingsBS.Default.AttrAreaApartTotal);
            AreaApartTotal = CalcAreaApartTotal();            
        }

        private double CalcAreaApartTotal ()
        {
            var res = AreaLive * (Floors - 1);
            if (Floors >= 30 && !IsIgnoreReductionFactor(Name))
            {
                res *= 0.92;
            }
            return res;
        }

        private bool IsIgnoreReductionFactor (string name)
        {
            return ReductionFactorIgnoringNamesBS.Contains(name, StringComparer.OrdinalIgnoreCase);
        }
    }
}