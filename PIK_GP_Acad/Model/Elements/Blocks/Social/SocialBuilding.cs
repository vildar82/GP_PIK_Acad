using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using AcadLib.Errors;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Elements.InfraworksExport;
using PIK_GP_Acad.OD;
using PIK_GP_Acad.OD.Records;

namespace PIK_GP_Acad.Elements.Blocks.Social
{
    /// <summary>
    /// Блок социального назначения - СОШ, ДОШ и т.п
    /// </summary>
    public abstract class SocialBuilding : BuildingBlockBase, IBuilding, IInfraworksExport
    {
        const string ParamPlaces = "^Количество мест"; // параметр видимости. Должен начинаться с числа мест
        const string LayerCoverage = "_ГП_проект проездов";
                
        /// <summary>
        /// Кол мест
        /// </summary>
        public int Places { get; set; }        
        /// <summary>
        /// Тип - "ДОО на 100 мест".
        /// </summary>
        public string Type { get; set; }                        

        public SocialBuilding (BlockReference blRef, string blName, string layerPlContour) : base(blRef, blName)
        {            
            Type = BlockBase.GetPropValue<string>("^ТИП", exactMatch: false);            
            Height = Floors * 4;            
            Places = GetPlaces(ParamPlaces);
            BuildingType = BuildingTypeEnum.Social;

            // Полилиния контура здания
            var plContour = BlockBase.FindPolylineInLayer(layerPlContour).FirstOrDefault();
            if (plContour == null)
            {
                Inspector.AddError($"Не определена полилиния котура здания на слое {layerPlContour}.", System.Drawing.SystemIcons.Warning);
            }
            else
            {
                IdPlContour = plContour.Id;
            }            
        }       

        private int GetPlaces (string paramName)
        {
            int value =0;
            string input = BlockBase.GetPropValue<string>(paramName, exactMatch: false); // 100 круглый            
            var resInt = AcadLib.Strings.StringHelper.GetStartInteger(input);
            if (resInt.Success)
            {
                value = resInt.Value;
            }
            else
            {
                BlockBase.AddError($"Не определено '{paramName}' из параметра - {input}");
            }
            return value;
        }

        public List<IODRecord> GetODRecords ()
        {
            List<IODRecord> recs = new List<IODRecord>();

            // Запись ODBuilding
            var odBuild = ODBuilding.GetRecord(BlockBase, IdPlContour,OD.Records.BuildingType.Social, Height);
            recs.Add(odBuild);

            // Запись ODCoverage
            var recsCoverage = ODCoverage.GetRecords(BlockBase, LayerCoverage, CoverageType.SideWalk);
            recs.AddRange(recsCoverage);

            return recs;
        }        
    }
}
