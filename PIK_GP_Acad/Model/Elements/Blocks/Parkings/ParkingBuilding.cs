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
using PIK_GP_Acad.Parkings;

namespace PIK_GP_Acad.Elements.Blocks.Parkings
{
    /// <summary>
    /// КП_Паркинг - здание
    /// </summary>
    public class ParkingBuilding : BuildingBlockBase, IParking, IBuilding, IInfraworksExport
    {
        public const string BlockName = "КП_Паркинг";

        private const string ParamPlaces = "^МАШИНОМЕСТА";        

        const string LayerContour = "_ГП_здания паркингов";
        const string LayerCoverage = "_ГП_проект проездов";

        public ObjectId IdBlRef { get; set; }        
        public int Places { get; set; }
        public int InvalidPlaces { get; set; }                               

        public ParkingBuilding (BlockReference blRef, string blName) : base(blRef, blName)
        {
            IdBlRef = blRef.Id;            
            Height = Floors * 3;
            FriendlyTypeName = "Паркинг";
            BuildingType = BuildingTypeEnum.Garage;

            var valPlaces = BlockBase.GetPropValue<string>(ParamPlaces, exactMatch:false);
            var resPlaces = AcadLib.Strings.StringHelper.GetStartInteger(valPlaces);
            if (resPlaces.Success)
            {
                Places = resPlaces.Value;
            }
            else
            {
                BlockBase.AddError($"Не определено кол машиномест из параметра {ParamPlaces} = {valPlaces}");
            }

            // Полилиния контура
            var plContour = BlockBase.FindPolylineInLayer(LayerContour).FirstOrDefault();
            if (plContour== null)
            {
                Inspector.AddError($"Не определена полилиния контура здания парковки на слое {LayerContour}.",
                    IdBlRef, System.Drawing.SystemIcons.Warning);
            }
            else
            {
                IdPlContour = plContour.Id;
            }
        }

        public void Calc ()
        {            
        }        

        public List<IODRecord> GetODRecords ()
        {
            List<IODRecord> recs = new List<IODRecord>();

            // Запись ODBuilding
            var odBuild = ODBuilding.GetRecord(BlockBase, IdPlContour, OD.Records.BuildingType.Garage, Height);
            recs.Add(odBuild);

            // Запись ODCoverage
            var recsCoverage = ODCoverage.GetRecords(BlockBase, LayerCoverage, CoverageType.SideWalk);
            recs.AddRange(recsCoverage);

            return recs;
        }        
    }
}
