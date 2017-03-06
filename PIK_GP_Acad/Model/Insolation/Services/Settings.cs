using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    public class Settings : ISettings
    {
        private static string fileSettings = AcadLib.IO.Path.GetSharedFile("Insolation", "Settings.xml");
        private static Settings settings;
        public ObservableCollection<InsRegion> Regions { get; set; }
        
        public ObservableCollection<InsRequirement> InsRequirements { get; set; }

        public Settings ()
        {            
        }

        public void Load ()
        {
            settings = Default();            
            Regions = settings.Regions==null? DefaultRegions(): settings.Regions;            
            InsRequirements = settings.InsRequirements == null ? DefaultInsRequirements() : settings.InsRequirements;            
        }

        public void Save ()
        {            
            //settings.Save(fileSettings, SerializationMode.Xml, new Catel.Runtime.Serialization.SerializationConfiguration());
        }

        private Settings Default ()
        {
            Settings settings = new Settings();
            settings.Regions = DefaultRegions();            
            settings.InsRequirements = DefaultInsRequirements();
            return settings;
        }

        public static ObservableCollection<InsRegion> DefaultRegions ()
        {            
            ObservableCollection<InsRegion> regions = null;
            if (regions == null || regions.Count == 0)
            {
                regions = new ObservableCollection<InsRegion> {
                    new InsRegion (RegionEnum.Central,"Калужская область", "Калуга",54 ),
                    new InsRegion (RegionEnum.Central,"Калужская область", "Обниск", 55 ),
                    new InsRegion (RegionEnum.Central,"Московская область", "Москва", 55 ),
                    new InsRegion (RegionEnum.Central,"Московская область", "Одинцово",55 ),
                    new InsRegion (RegionEnum.Central,"Новосибирская область","Новосибирск",55 ),
                    new InsRegion (RegionEnum.Central,"Ярославская область","Ярославль",57 ),
                    new InsRegion (RegionEnum.Central,"Свердловская область","Екатеринбург",56.5 )                    
                };
            }            
            return regions;
        }

        public static ObservableCollection<InsRequirement> DefaultInsRequirements ()
        {
            var reqs = new ObservableCollection<InsRequirement> {
                new InsRequirement() { Type = InsRequirementEnum.D, Color = Color.Aqua },
                new InsRequirement() { Type = InsRequirementEnum.C, Color = Color.Green },
                new InsRequirement() { Type = InsRequirementEnum.B, Color = Color.Yellow },
                new InsRequirement() { Type = InsRequirementEnum.A, Color = Color.Red },
                new InsRequirement() { Type = InsRequirementEnum.A1, Color = Color.Red }
            };
            return reqs;
        }
    }
}
