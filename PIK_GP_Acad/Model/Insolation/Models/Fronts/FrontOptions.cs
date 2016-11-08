using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Насройки расчета фронотов
    /// </summary>
    public class FrontOptions : ModelBase, IExtDataSave, ITypedDataValues
    {
        public FrontOptions()
        {

        }

        /// <summary>
        /// Шаг расчетной точки по фронту
        /// </summary>
        public double StepCalcPointInFront { get; set; }=0.5;
        /// <summary>
        /// Толщина линии фрона
        /// </summary>
        public double LineFrontWidth { get; set; } = 0.8;
        public string FrontLineLayer { get; set; } = "sapr_ins_front";

        public static FrontOptions Default ()
        {
            var opt = new FrontOptions();
            //{
            //    StepCalcPointInFront = 0.3,
            //    LineFrontWidth = 0.6,
            //    FrontLineLayer = "sapr_ins_front"
            //};
            return opt;
        }

        public System.Drawing.Color GetFrontColor (InsRequirementEnum insValue)
        {
            switch (insValue)
            {
                case InsRequirementEnum.None:
                    return System.Drawing.Color.Gray;
                case InsRequirementEnum.D:                    
                case InsRequirementEnum.C:
                    return System.Drawing.Color.Green;
                case InsRequirementEnum.B:
                    return System.Drawing.Color.Yellow;
                case InsRequirementEnum.A:
                    return System.Drawing.Color.Red;                
            }
            return System.Drawing.Color.Gray;
        }

        public DicED GetExtDic (Document doc)
        {
            var dicOpt = new DicED();
            dicOpt.AddRec("FrontOptionsRec", GetDataValues(doc));
            return dicOpt;
        }
        public void SetExtDic (DicED dicOpt, Document doc)
        {
            SetDataValues(dicOpt.GetRec("FrontOptionsRec")?.Values, doc);
        }
        public List<TypedValue> GetDataValues (Document doc)
        {
            return new List<TypedValue> {                
                TypedValueExt.GetTvExtData(StepCalcPointInFront),
                TypedValueExt.GetTvExtData(LineFrontWidth),
                TypedValueExt.GetTvExtData(FrontLineLayer),
            };
        }
        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            if (values == null || values.Count != 3)
            {
                // Дефолт

            }
            else
            {
                int index = 0;
                StepCalcPointInFront = TypedValueExt.GetTvValue<double>(values[index++]);
                LineFrontWidth = TypedValueExt.GetTvValue<double>(values[index++]);
                FrontLineLayer = TypedValueExt.GetTvValue<string>(values[index++]);
            }
        }
    }
}
