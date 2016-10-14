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
using Catel.Data;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{    
    public class InsOptions : ModelBase, IExtDataSave, ITypedDataValues
    {
        public InsOptions ()
        {            
        }

        public byte Transparence { get; set; }/* = 120;        */
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
        public double SunCalcAngleStart { get; set; }/* = 15.0;*/
        /// <summary>
        /// Конечный расчетный угол (минус последний час) [град]. Заход(центр) = 180град -15
        /// </summary>
        public double SunCalcAngleEnd { get; set; }/* = 165.0;        */

        public static InsOptions Default ()
        {
            InsOptions defaultOptions = new InsOptions {
                Transparence = 120, TileSize = 1, Region = InsService.Settings.Regions[0],
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
                Transparence = defOpt.Transparence;
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
                TypedValueExt.GetTvExtData(Transparence),
                TypedValueExt.GetTvExtData(TileSize),
                TypedValueExt.GetTvExtData(ShadowDegreeStep),
                TypedValueExt.GetTvExtData(SunCalcAngleStart),
                TypedValueExt.GetTvExtData(SunCalcAngleEnd)
            };
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            if (values == null || values.Count != 5)
            {
                // Дефолтные настройки
                var defOpt = Default();
                Transparence = defOpt.Transparence;
                TileSize = defOpt.TileSize;
                ShadowDegreeStep = defOpt.ShadowDegreeStep;
                SunCalcAngleEnd = defOpt.SunCalcAngleEnd;
                SunCalcAngleStart = defOpt.SunCalcAngleStart;
            }
            else
            {
                int index = 0;
                Transparence = values[index++].GetTvValue<byte>();
                TileSize = values[index++].GetTvValue<int>();
                ShadowDegreeStep = values[index++].GetTvValue<int>();
                SunCalcAngleStart = values[index++].GetTvValue<double>();
                SunCalcAngleEnd = values[index++].GetTvValue<double>();
            }
        }
    }
}
