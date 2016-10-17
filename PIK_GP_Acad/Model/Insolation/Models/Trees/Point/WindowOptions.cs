using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Catel.Data;
using Catel.Runtime.Serialization;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Настройки расчетной точки
    /// </summary>    
    public class WindowOptions : ModelBase, IExtDataSave, ITypedDataValues
    {
        public WindowOptions () { }

        /// <summary>
        /// Ширина окна, м
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// Тип конструкции
        /// </summary>        
        public WindowConstruction Construction { get; set; }
        /// <summary>
        /// Глубина четверти
        /// </summary>
        public double Quarter { get; set; } 
        /// <summary>
        /// Угол который ограничивает инсоляцию при расчете точки (симметрично с двух сотор) 
        /// [рад]
        /// </summary>
        public double ShadowAngle { get; set; }    
        /// <summary>
        /// Если значение угла введено пользователем
        /// </summary>
        public bool IsCustomAngle { get; set; }              

        /// <summary>
        /// Определение теневого угла - по введенным значениям
        /// </summary>        
        public double CalcShadowAngle()
        {
            if (Construction == null) return 0;                    
            double b = Math.Atan2(Construction.Depth + Quarter, Width + 0.065);
            return b;
        }       

        protected override void OnPropertyChanged (AdvancedPropertyChangedEventArgs e)
        {            
            if (!IsCustomAngle && e.PropertyName != nameof(ShadowAngle))
            {
                ShadowAngle = CalcShadowAngle();
            }            
        }

        public static WindowOptions Default ()
        {
            return new WindowOptions {
                Width = 1.5,
                Quarter = 0.07,                                
                Construction = WindowConstruction.WindowConstructions[0] };
        }

        public DicED GetExtDic (Document doc)
        {
            DicED dicWinOpt = new DicED();
            dicWinOpt.AddRec("WindowOptionsRec", GetDataValues(doc));
            dicWinOpt.AddRec("WindowConstruction", Construction.GetDataValues(doc));
            return dicWinOpt;
        }

        public void SetExtDic (DicED dic, Document doc)
        {            
            SetDataValues(dic.GetRec("WindowOptionsRec")?.Values, doc);
            var constr = new WindowConstruction();
            constr.SetDataValues(dic.GetRec("WindowConstruction")?.Values, doc);
            
            Construction = WindowConstruction.GetStandart(constr);
            
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            return new List<TypedValue> {
                TypedValueExt.GetTvExtData(Width),
                TypedValueExt.GetTvExtData(Quarter),
                TypedValueExt.GetTvExtData(ShadowAngle),
                TypedValueExt.GetTvExtData(IsCustomAngle),
            };
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            if (values == null || values.Count != 4)
            {
                // default
                var defWin = Default();
                Width = defWin.Width;
                Quarter = defWin.Quarter;
                ShadowAngle = defWin.ShadowAngle;
                IsCustomAngle = defWin.IsCustomAngle;
            }
            else
            {
                int index = 0;
                Width = values[index++].GetTvValue<double>();
                Quarter = values[index++].GetTvValue<double>();
                ShadowAngle = values[index++].GetTvValue<double>();
                IsCustomAngle = values[index++].GetTvValue<bool>();
            }
        }
    }
}
