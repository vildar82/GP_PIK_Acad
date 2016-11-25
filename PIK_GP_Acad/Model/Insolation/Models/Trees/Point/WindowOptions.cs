using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Настройки расчетной точки
    /// </summary>    
    public class WindowOptions : ModelBase, IExtDataSave, ITypedDataValues
    {
        public WindowOptions ()
        {            
        }

        /// <summary>
        /// Ширина окна, м
        /// </summary>
        public double Width { get { return width; } set { width = value; RaisePropertyChanged(); } }
        double width;
        /// <summary>
        /// Тип конструкции
        /// </summary>        
        public WindowConstruction Construction { get { return construction; } set { construction = value; RaisePropertyChanged(); } }
        WindowConstruction construction;

        /// <summary>
        /// Глубина четверти
        /// </summary>
        public double Quarter { get { return quarter; } set { quarter = value; RaisePropertyChanged(); } }
        double quarter;
        /// <summary>
        /// Угол который ограничивает инсоляцию при расчете точки (симметрично с двух сотор) 
        /// [рад]
        /// </summary>
        public double ShadowAngle { get { return shadowAngle; } set { shadowAngle = value; RaisePropertyChanged(); } }
        double shadowAngle;
        /// <summary>
        /// Если значение угла введено пользователем
        /// </summary>
        public bool IsCustomAngle { get { return isCustomAngle; } set { isCustomAngle = value; RaisePropertyChanged(); } }
        bool isCustomAngle;

        /// <summary>
        /// Определение теневого угла - по введенным значениям
        /// </summary>        
        public double CalcShadowAngle()
        {
            if (Construction == null) return 0;                    
            double b = Math.Atan2(Construction.Depth + Quarter, Width + 0.065);
            return b;
        }

        protected override void OnPropertyChanged (PropertyChangedEventArgs e)
        {
            if (!IsCustomAngle && e.PropertyName != nameof(ShadowAngle))
            {
                ShadowAngle = CalcShadowAngle();
            }
        }

        public WindowOptions Copy ()
        {
            return (WindowOptions)MemberwiseClone();
        }

        public static WindowOptions Default ()
        {
            return new WindowOptions {
                Width = 1.2,
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
