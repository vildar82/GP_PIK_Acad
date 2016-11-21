using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using AcadLib.Errors;
using AcadLib.RTree.SpatialIndex;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Elements.InfraworksExport;
using PIK_GP_Acad.KP.KP_BlockSection;
using PIK_GP_Acad.OD;
using PIK_GP_Acad.OD.Records;

namespace PIK_GP_Acad.Elements.Blocks.BlockSection
{
    /// <summary>
    /// блок-секции концепции - пока общий класс для Рядовой и Угловой
    /// </summary>
    public class BlockSectionKP : BlockSectionBase
    {
        public const string BlockNameMatch = "ГП_К_Секция";       

        public BlockSectionKP(BlockReference blRef, string blName) : base (blRef, blName)
        {
            // Определить параметры блок-секции: площадь,этажность            
            Define(blRef);            
        }

        protected override void Define (BlockReference blRef)
        {
            // Определение этажности по атрибуту
            Floors = BlockBase.GetPropValue<int>(OptionsKPBS.Instance.BlockSectionAtrFloor, exactMatch: false);

            base.Define(blRef);
        }

        /// <summary>
        /// Для экспорта в Infraworks - запись OD к полилинии контура
        /// </summary>        
        public List<IODRecord> GetODRecords ()
        {
            var odBuild = ODBuilding.GetRecord(BlockBase, IdPlContour,OD.Records.BuildingType.Live, Height);
            return new List<IODRecord> { odBuild };
        }
    }
}
