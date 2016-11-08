﻿using System;
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
    public class ParkingBuilding : BlockBase, IParking, IBuilding, IInfraworksExport
    {
        public const string BlockName = "КП_Паркинг";

        private const string ParamPlaces = "^МАШИНОМЕСТА";
        private const string ParamFloors = "^ЭТАЖНОСТЬ";

        const string LayerContour = "_ГП_здания паркингов";
        const string LayerCoverage = "_ГП_проект проездов";

        public ObjectId IdEnt { get; set; }
        public int Floors { get; set; }
        public int Places { get; set; }
        public int InvalidPlaces { get; set; }
        public Extents3d ExtentsInModel { get; set; }
        
        public ObjectId IdPlContour { get; set; }        
        public int Height { get; set; }

        public BuildingTypeEnum BuildingType { get; set; } = BuildingTypeEnum.Garage;

        public string HouseName { get; set; }

        public string PluginName { get; set; }

        public ParkingBuilding (BlockReference blRef, string blName) : base(blRef, blName)
        {
            IdEnt = blRef.Id;
            ExtentsInModel = Bounds.Value;
            Floors = GetPropValue<int>(ParamFloors, exactMatch: false);
            Height = Floors * 3;

            var valPlaces = GetPropValue<string>(ParamPlaces, exactMatch:false);
            var resPlaces = AcadLib.Strings.StringHelper.GetStartInteger(valPlaces);
            if (resPlaces.Success)
            {
                Places = resPlaces.Value;
            }
            else
            {
                AddError($"Не определено кол машиномест из параметра {ParamPlaces} = {valPlaces}");
            }

            // Полилиния контура
            var plContour = FindPolylineInLayer(LayerContour).FirstOrDefault();
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

        public Polyline GetContourInModel ()
        {
            using (var pl = IdPlContour.Open(OpenMode.ForRead) as Polyline)
            {
                var plCopy = (Polyline)pl.Clone();
                plCopy.TransformBy(Transform);
                if (plCopy.Elevation != 0)
                    plCopy.Elevation = 0;
                return plCopy;
            }
        }

        public List<IODRecord> GetODRecords ()
        {
            List<IODRecord> recs = new List<IODRecord>();

            // Запись ODBuilding
            var odBuild = ODBuilding.GetRecord(this, IdPlContour, OD.Records.BuildingType.Garage, Height);
            recs.Add(odBuild);

            // Запись ODCoverage
            var recsCoverage = ODCoverage.GetRecords(this, LayerCoverage, CoverageType.SideWalk);
            recs.AddRange(recsCoverage);

            return recs;
        }

        public DBObject GetDBObject ()
        {
            throw new NotImplementedException();
        }

        public DicED GetExtDic (Document doc)
        {
            throw new NotImplementedException();
        }

        public void SetExtDic (DicED dicEd, Document doc)
        {
            throw new NotImplementedException();
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            throw new NotImplementedException();
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            throw new NotImplementedException();
        }
    }
}
