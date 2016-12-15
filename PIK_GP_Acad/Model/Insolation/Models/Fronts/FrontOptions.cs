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
using MicroMvvm;

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
        public double StepCalcPointInFront { get; set; }=0.4;
        /// <summary>
        /// Толщина линии фрона
        /// </summary>
        public double LineFrontWidth { get; set; } = 0.8;
        //public string FrontLineLayer { get; set; } = "sapr_ins_front";
        

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
                case InsRequirementEnum.A1:// Немного не дотягивает до B (продолж.непр.инс >=1ч.22.мин, но меньше 1ч.30мин.)
                    return System.Drawing.Color.HotPink;
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
            SetDataValues(dicOpt?.GetRec("FrontOptionsRec")?.Values, doc);
        }
        public List<TypedValue> GetDataValues (Document doc)
        {
            var tvk = new TypedValueExtKit();
            tvk.Add("StepCalcPointInFront", StepCalcPointInFront);
            tvk.Add("LineFrontWidth", LineFrontWidth);            
            return tvk.Values;            
        }
        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            var dictValues = values?.ToDictionary();
            StepCalcPointInFront = dictValues.GetValue("StepCalcPointInFront", 0.4);
            LineFrontWidth = dictValues.GetValue("LineFrontWidth", 0.8);                        
        }
    }
}
