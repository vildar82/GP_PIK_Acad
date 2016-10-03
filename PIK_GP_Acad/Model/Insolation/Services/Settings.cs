﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    public class Settings : SavableModelBase<Settings>, ISettings
    {
        private static string fileSettings = AcadLib.IO.Path.GetSharedFile("Insolation", "Settings.xml");
        private static Settings settings;
        public ObservableCollection<InsRegion> Regions { get; set; }
        public ObservableCollection<TreeVisualOption> TreeVisualOptions { get; set; }
        public ObservableCollection<InsRequirement> InsRequirements { get; set; }

        public Settings ()
        {            
        }

        public void Load ()
        {   
            if (!File.Exists(fileSettings))
            {
                settings = Default();
            }
            else
            {
                using (var fileStream = File.Open(fileSettings, FileMode.Open))
                {
                    settings = Load(fileStream, SerializationMode.Xml,new Catel.Runtime.Serialization.SerializationConfiguration());                                        
                }
            }            
            Regions = settings.Regions==null? DefaultRegions(): settings.Regions;
            TreeVisualOptions = settings.TreeVisualOptions == null ? DefaultTreeVisualOptions() : settings.TreeVisualOptions;
            InsRequirements = settings.InsRequirements == null ? DefaultInsRequirements() : settings.InsRequirements;            
        }

        public void Save ()
        {            
            settings.Save(fileSettings, SerializationMode.Xml, new Catel.Runtime.Serialization.SerializationConfiguration());
        }

        private Settings Default ()
        {
            Settings settings = new Settings();
            settings.Regions = DefaultRegions();
            settings.TreeVisualOptions = DefaultTreeVisualOptions();
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
                };
            }            
            return regions;
        }

        public static ObservableCollection<TreeVisualOption> DefaultTreeVisualOptions ()
        {
            ObservableCollection<TreeVisualOption> visuals = new ObservableCollection<TreeVisualOption> {
                new TreeVisualOption (Color.FromArgb(205, 32, 39), 35),
                new TreeVisualOption (Color.FromArgb(241, 235, 31), 55),
                new TreeVisualOption (Color.FromArgb(19, 155, 72), 75),
            };
            return visuals;
        }

        public static ObservableCollection<InsRequirement> DefaultInsRequirements ()
        {
            var reqs = new ObservableCollection<InsRequirement> {
                new InsRequirement() { Type = InsRequirementEnum.D, Color = Color.Blue },
                new InsRequirement() { Type = InsRequirementEnum.C, Color = Color.Green },
                new InsRequirement() { Type = InsRequirementEnum.B, Color = Color.Yellow },
                new InsRequirement() { Type = InsRequirementEnum.A, Color = Color.Red }
            };
            return reqs;
        }
    }
}