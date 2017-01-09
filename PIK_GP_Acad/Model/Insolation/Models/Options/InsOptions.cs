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
using MicroMvvm;

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
        /// <summary>
        /// Проверка наложения домов - включена/выключена
        /// </summary>
        public bool EnableCheckDublicates { get; set; }        

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
            SetDataValues(dicOpt?.GetRec("InsOptionsRec")?.Values, doc);
            // Регион            
            Region = new InsRegion();
            Region.SetExtDic(dicOpt?.GetInner("InsRegion"), doc);
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            var tvk = new TypedValueExtKit();
            tvk.Add("TileSize", TileSize);
            tvk.Add("ShadowDegreeStep", ShadowDegreeStep);
            tvk.Add("ProjectId", Project?.Id ?? 0);
            tvk.Add("EnableCheckDublicates", EnableCheckDublicates);
            return tvk.Values;            
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            var dictValues = values?.ToDictionary();            
            TileSize = dictValues.GetValue("TileSize", 1);
            ShadowDegreeStep = dictValues.GetValue("ShadowDegreeStep", 1);
            var id = dictValues.GetValue("ProjectId", 0);
            Project = DbService.FindProject(id);
            EnableCheckDublicates = dictValues.GetValue("EnableCheckDublicates", true);
        }
    }
}
