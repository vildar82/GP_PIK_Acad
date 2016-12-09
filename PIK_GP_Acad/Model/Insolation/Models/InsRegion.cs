using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Services;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.Models
{    
    public class InsRegion : ModelBase, IEquatable<InsRegion>, IExtDataSave, ITypedDataValues
    {        
        private static string FileRegions = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder, 
            @"ГП\Insolation\Regions.xml");
        
        public RegionEnum RegionPart { get { return regionPart; } set { regionPart = value; RaisePropertyChanged(); } }
        RegionEnum regionPart;
        public string RegionName { get { return regionName; } set { regionName = value; RaisePropertyChanged(); } }
        string regionName;
        public string City { get { return city; } set { city = value; RaisePropertyChanged(); } }
        string city;
        public double Latitude { get { return latitude; } set { latitude = value; RaisePropertyChanged(); } }
        double latitude;

        public InsRegion () : base() { }

        public InsRegion (RegionEnum regPart, string regName, string city, double latitude) : base()
        {
            RegionPart = regPart;
            RegionName = regName;
            City = city;
            Latitude = latitude;            
        }

        public bool Equals (InsRegion other)
        {
            if (other == null) return false;
            var res = RegionPart == other.RegionPart &&
                RegionName == other.RegionName &&
                City == other.City &&
                Latitude == other.Latitude;
            return res;
        }

        public override int GetHashCode ()
        {
            return RegionPart.GetHashCode() ^ Latitude.GetHashCode();
        }

        public DicED GetExtDic (Document doc)
        {
            DicED dicReg = new DicED();            
            dicReg.AddRec("InsRegionRec", GetDataValues(doc));
            return dicReg;
        }

        public void SetExtDic (DicED dicReg, Document doc)
        {            
            SetDataValues(dicReg.GetRec("InsRegionRec")?.Values, doc);
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            return new List<TypedValue> {
                TypedValueExt.GetTvExtData((int)RegionPart),
                TypedValueExt.GetTvExtData(RegionName),
                TypedValueExt.GetTvExtData(City),
                TypedValueExt.GetTvExtData(Latitude)
            };
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            if (values == null || values.Count != 4)
            {
                // default
                var regDef = InsService.Settings.Regions[0];
                RegionPart = regDef.RegionPart;
                RegionName = regDef.RegionName;
                City = regDef.City;
                Latitude = regDef.Latitude;
            }
            else
            {
                int index = 0;
                RegionPart = (RegionEnum)values[index++].GetTvValue<int>();
                RegionName = values[index++].GetTvValue<string>();
                City = values[index++].GetTvValue<string>();
                Latitude = values[index++].GetTvValue<double>();
            }
        }
    }
}
