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
            SetDataValues(dicReg?.GetRec("InsRegionRec")?.Values, doc);
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            var tvk = new TypedValueExtKit();
            tvk.Add("RegionPart", RegionPart);
            tvk.Add("RegionName", RegionName);
            tvk.Add("City", City);
            tvk.Add("Latitude", Latitude);
            return tvk.Values;            
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            var dictValues = values?.ToDictionary();
            var regDef = InsService.Settings.Regions.FirstOrDefault(r => r.City.Equals("Москва", StringComparison.OrdinalIgnoreCase)) 
                            ?? InsService.Settings.Regions[0];            
            RegionPart = dictValues.GetValue("RegionPart", regDef.RegionPart);
            RegionName = dictValues.GetValue("RegionName", regDef.RegionName);
            City = dictValues.GetValue("City", regDef.City);
            Latitude = dictValues.GetValue("Latitude", regDef.Latitude);
        }
    }
}
