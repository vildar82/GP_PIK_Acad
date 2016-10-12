using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using AcadLib.Errors;
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
    public abstract class SocialBuilding : BlockBase, IBuilding, IInfraworksExport
    {
        const string ParamPlaces = "^Количество мест"; // параметр видимости. Должен начинаться с числа мест
        const string LayerCoverage = "_ГП_проект проездов";

        public ObjectId IdEnt { get; set; }
        /// <summary>
        /// Кол мест
        /// </summary>
        public int Places { get; set; }
        /// <summary>
        /// Этажей
        /// </summary>
        public int Floors { get; set; }
        /// <summary>
        /// Тип - "ДОО на 100 мест".
        /// </summary>
        public string Type { get; set; }
        public Extents3d ExtentsInModel { get; set; }        

        public int Height { get; set; }
        public abstract ObjectId IdPlContour { get; set;}

        public BuildingTypeEnum BuildingType { get; set; } = BuildingTypeEnum.Social;

        public SocialBuilding (BlockReference blRef, string blName, string layerPlContour) : base(blRef, blName)
        {
            IdEnt = blRef.Id;
            Type = GetPropValue<string>("^ТИП", exactMatch: false);
            Floors = GetPropValue<int>("^ЭТАЖНОСТЬ", exactMatch: false);
            Height = Floors * 4;
            ExtentsInModel = Bounds.Value;
            Places = GetPlaces(ParamPlaces);

            // Полилиния контура здания
            var plContour = FindPolylineInLayer(layerPlContour).FirstOrDefault();
            if (plContour == null)
            {
                Inspector.AddError($"Не определена полилиния котура здания на слое {layerPlContour}.", System.Drawing.SystemIcons.Warning);
            }
            else
            {
                IdPlContour = plContour.Id;
            }
            
        }

        public Polyline GetContourInModel ()
        {
            var pl = IdPlContour.GetObject(OpenMode.ForRead) as Polyline;
            var plCopy = (Polyline)pl.Clone();
            plCopy.TransformBy(Transform);
            return plCopy;
        }

        private int GetPlaces (string paramName)
        {
            int value =0;
            string input = GetPropValue<string>(paramName, exactMatch: false); // 100 круглый            
            var resInt = AcadLib.Strings.StringHelper.GetStartInteger(input);
            if (resInt.Success)
            {
                value = resInt.Value;
            }
            else
            {
                AddError($"Не определено '{paramName}' из параметра - {input}");
            }
            return value;
        }

        public List<IODRecord> GetODRecords ()
        {
            List<IODRecord> recs = new List<IODRecord>();

            // Запись ODBuilding
            var odBuild = ODBuilding.GetRecord(this, IdPlContour,OD.Records.BuildingType.Social, Height);
            recs.Add(odBuild);

            // Запись ODCoverage
            var recsCoverage = ODCoverage.GetRecords(this, LayerCoverage, CoverageType.SideWalk);
            recs.AddRange(recsCoverage);

            return recs;
        }
    }
}
