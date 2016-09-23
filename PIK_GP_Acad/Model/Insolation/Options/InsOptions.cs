using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using AcadLib;
using PIK_GP_Acad.Model.Insolation;

namespace PIK_GP_Acad.Insolation.Options
{
    public class InsOptions
    {
        private static string FileRegions = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder, @"ГП\Insolation\Regions.xml");
        public static List<RegionOptions> Regions { get; private set; } = LoadRegions();

        public byte Transparence { get; set; } = 120;
        public List<VisualOption> VisualOptions { get; set; }                 
        /// <summary>
        /// Размер ячейки карты - квадрата, на который разбивается вся карта
        /// </summary>
        public int TileSize { get; set; } = 1;
        public RegionOptions Region { get; set; }        
        /// <summary>
        /// Шаг угла луча (в градусах) при определении теней
        /// </summary>
        public int ShadowDegreeStep { get; set; } = 1;
        /// <summary>
        /// Начальный расчетный угол (пропуская первый час). Восход = 0град.
        /// </summary>
        public double SunCalcAngleStart { get; set; } = 15.0;
        /// <summary>
        /// Конечный расчетный угол (минус последний час). Заход = 180град.
        /// </summary>
        public double SunCalcAngleEnd { get; set; } = 165.0;        

        public InsOptions ()
        {
            Region = Regions[0];
            VisualOptions = DefaultVisualOptions();
        }        

        private static List<RegionOptions> LoadRegions ()
        {
            List<RegionOptions> regions = AcadLib.Files.SerializerXml.Load<List<RegionOptions>>(FileRegions);
            if (regions.Count == 0)
            {
                regions = new List<RegionOptions> {
                    new RegionOptions(RegionEnum.Central, "Калужская область", "Калуга", 54),
                    new RegionOptions(RegionEnum.Central, "Калужская область", "Обниск", 55),
                    new RegionOptions(RegionEnum.Central, "Московская область", "Москва", 55),
                    new RegionOptions(RegionEnum.Central, "Московская область", "Одинцово", 55),
                    new RegionOptions(RegionEnum.Central, "Новосибирская область", "Новосибирск", 55),
                    new RegionOptions(RegionEnum.Central, "Ярославская область", "Ярославль", 57),
                };
            }
            AcadLib.Files.SerializerXml.Save(FileRegions, regions);
            return regions;
        }

        public static List<VisualOption> DefaultVisualOptions ()
        {
            List<VisualOption> visuals = new List<VisualOption> {
                new VisualOption (Color.FromRgb(205, 32, 39), 35),
                new VisualOption (Color.FromRgb(241, 235, 31), 55),
                new VisualOption (Color.FromRgb(19, 155, 72), 75),
            };            
            return visuals;
        }
    }
}
