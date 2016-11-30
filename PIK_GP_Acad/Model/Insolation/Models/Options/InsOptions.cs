using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using AcadLib;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_DB_Projects;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{    
    public class InsOptions : ModelBase, IExtDataSave, ITypedDataValues
    {
        public InsOptions ()
        {            
        }

        //public byte Transparence { get; set; }/* = 120;        */
        /// <summary>
        /// Размер ячейки карты - квадрата, на который разбивается вся карта
        /// </summary>
        public int TileSize { get; set; }/* = 1;*/
        public InsRegion Region { get; set; }/* = InsService.Settings.Regions[0];*/
        /// <summary>
        /// Шаг угла луча (в градусах) при определении теней
        /// </summary>
        public int ShadowDegreeStep { get; set; }/* = 1;*/
        /// <summary>
        /// Начальный расчетный угол (пропуская первый час) [град]. Восход(центр) = 0град + 15
        /// </summary>
        public double SunCalcAngleStart { get; set; } = 15.0;
        /// <summary>
        /// Конечный расчетный угол (минус последний час) [град]. Заход(центр) = 180град -15
        /// </summary>
        public double SunCalcAngleEnd { get; set; }= 165.0;       
        /// <summary>
        /// Проект (по базе)
        /// </summary>
        public ProjectMDM Project { get { return project; } set { project = value; RaisePropertyChanged(); } }
        ProjectMDM project;

        public static InsOptions Default ()
        {
            var defaultRegion = InsService.Settings.Regions
                .FirstOrDefault(r => r.City.Equals("Москва", StringComparison.OrdinalIgnoreCase)) ?? InsService.Settings.Regions[0];            
            InsOptions defaultOptions = new InsOptions {
                TileSize = 1, Region = defaultRegion,
                ShadowDegreeStep = 1, SunCalcAngleStart = 15.0, SunCalcAngleEnd = 165.0
            };
            return defaultOptions;
        }

        public DicED GetExtDic (Document doc)
        {
            DicED dicOpt = new DicED();            
            dicOpt.AddRec("InsOptionsRec", GetDataValues(doc));            
            dicOpt.AddInner("InsRegion", Region.GetExtDic(doc));
            return dicOpt;
        }

        public void SetExtDic (DicED dicOpt, Document doc)
        {
            if (dicOpt== null)
            {
                // Default
                var defOpt = Default();                
                TileSize = defOpt.TileSize;
                Region = defOpt.Region;
                ShadowDegreeStep = defOpt.ShadowDegreeStep;
                SunCalcAngleStart = defOpt.SunCalcAngleStart;
                SunCalcAngleEnd = defOpt.SunCalcAngleEnd;
                return;
            }
            var recOpt = dicOpt.GetRec("InsOptionsRec");
            SetDataValues(recOpt?.Values, doc);
            // Регион            
            Region = new InsRegion();
            Region.SetExtDic(dicOpt.GetInner("InsRegion"), doc);
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            return new List<TypedValue> {                
                TypedValueExt.GetTvExtData(TileSize),
                TypedValueExt.GetTvExtData(ShadowDegreeStep),
                //TypedValueExt.GetTvExtData(SunCalcAngleStart),
                //TypedValueExt.GetTvExtData(SunCalcAngleEnd),
                TypedValueExt.GetTvExtData(Project?.Id ?? 0)
            };
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            if (values == null || values.Count != 3)
            {
                // Дефолтные настройки
                var defOpt = Default();                
                TileSize = defOpt.TileSize;
                ShadowDegreeStep = defOpt.ShadowDegreeStep;
                //SunCalcAngleEnd = defOpt.SunCalcAngleEnd;
                //SunCalcAngleStart = defOpt.SunCalcAngleStart;
            }
            else
            {
                int index = 0;                
                TileSize = values[index++].GetTvValue<int>();
                ShadowDegreeStep = values[index++].GetTvValue<int>();
                //SunCalcAngleStart = values[index++].GetTvValue<double>();
                //SunCalcAngleEnd = values[index++].GetTvValue<double>();
                var id = values[index++].GetTvValue<int>();
                Project = DbService.FindProject(id);
            }
        }
    }
}
