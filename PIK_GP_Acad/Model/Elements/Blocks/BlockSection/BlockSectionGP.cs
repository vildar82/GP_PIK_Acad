using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Blocks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.BlockSection_GP;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Elements.InfraworksExport;
using PIK_GP_Acad.OD;
using PIK_GP_Acad.OD.Records;

namespace PIK_GP_Acad.Elements.Blocks.BlockSection
{
    // Блок-секция
    public class BlockSectionGP : BlockSectionBase
    {        
        public const string BlockSectionPrefix = "ГП_Блок-секция";
        public const string AttrAreaApart = "КВЭТ";
        public const string AttrAreaApartTotal = "КВОБЩ";
        public const string AttrAreaBKFN = "БКФН";
        public const string AttrName = "НАИМЕНОВАНИЕ";
        public const string AttrNumberFloor = "ЭТАЖЕЙ";

        private List<string> ReductionFactorIgnoringNamesBS = new List<string> { "Б-13", "Б-14" };

        public BlockSectionGP(BlockReference blRef, string blName) : base(blRef, blName)
        {
            FriendlyTypeName = "б/с ПИК1";
            // Кол этажей
            //Floors = BlockBase.GetPropValue<int>(SettingsBS.Default.AttrNumberFloor);
            //Height = Floors * 3 + 3;
            // Наименование
            Name = BlockBase.GetPropValue<string>(AttrName);
            // Площадь БКФН
            AreaBKFN = BlockBase.GetPropValue<double>(AttrAreaBKFN);
            //if (Floors >= 30)
            //    AreaBKFN *= 0.92;
            // Площадь квартир на одном этаже
            AreaLive = BlockBase.GetPropValue<double>(AttrAreaApart);
            // Площадь квартир общая на секцию (по всем этажам кроме 1)
            //AreaApartTotal = GetPropValue<double>(SettingsBS.Default.AttrAreaApartTotal);
            AreaApartTotal = CalcAreaApartTotal();
        }

        /// <summary>
        /// Общая площадь квартир 
        /// </summary>
        public double AreaApartTotal { get; private set; }        
        /// <summary>
        /// Площадь БКФН 
        /// </summary>
        public double AreaBKFN { get; private set; }        
        /// <summary>
        /// Имя блок-секции (У-С-10)
        /// </summary>
        public string Name { get; private set; } = string.Empty;      
       
        private double CalcAreaApartTotal ()
        {
            var res = AreaLive * (Floors - 1);
            if (Floors >= 30 && !IsIgnoreReductionFactor(Name))
            {
                res *= 0.92;
            }
            return res;
        }

        /// <summary>
        /// Игнорирование понижающего коэфициента жилой площади по имени блок-секцмии (Имена блок секции дает Петров)
        /// </summary>
        /// <param name="name">Имя блок-секции</param>
        /// <returns>Игнорируется ли для этой секции понижающий коэффициент</returns>
        private bool IsIgnoreReductionFactor (string name)
        {
            return ReductionFactorIgnoringNamesBS.Contains(name, StringComparer.OrdinalIgnoreCase);
        }
    }
}