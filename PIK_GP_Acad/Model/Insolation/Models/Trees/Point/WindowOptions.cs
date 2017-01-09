﻿using System;
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
using MicroMvvm;
using NetLib;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Настройки окна
    /// </summary>    
    public class WindowOptions : ModelBase, IExtDataSave, ITypedDataValues, IEquatable<WindowOptions>
    {
        public WindowOptions ()
        {            
        }

        public static WindowOptions Default
        {
            get { return defaultWindow; }
        }
        private static WindowOptions defaultWindow = new WindowOptions
        {
            Width = 1.2,
            Quarter = 0.07,
            Construction = WindowConstruction.WindowConstructions[0]
        };

        /// <summary>
        /// Ширина окна, м
        /// </summary>
        public double Width { get { return width; }
            set { width = value; RaisePropertyChanged(); } }
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
        public double ShadowAngle { get { return shadowAngle; }
            set { shadowAngle = value; RaisePropertyChanged(); } }
        double shadowAngle;
        /// <summary>
        /// Если значение угла введено пользователем
        /// </summary>
        public bool IsCustomAngle { get { return isCustomAngle; } set { isCustomAngle = value; RaisePropertyChanged(); } }
        bool isCustomAngle;

        public static List<double> Quarters { get; set; } = new List<double> { 0.07, 0.13, 0.26 };
        

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
            base.OnPropertyChanged(e);
        }

        public WindowOptions Copy ()
        {
            return (WindowOptions)MemberwiseClone();
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
            SetDataValues(dic?.GetRec("WindowOptionsRec")?.Values, doc);
            var constr = new WindowConstruction();
            constr.SetDataValues(dic?.GetRec("WindowConstruction")?.Values, doc);            
            Construction = WindowConstruction.GetStandart(constr);            
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            var tvk = new TypedValueExtKit();
            tvk.Add("Width", Width);
            tvk.Add("Quarter", Quarter);
            tvk.Add("ShadowAngle", ShadowAngle);
            tvk.Add("IsCustomAngle", IsCustomAngle);
            return tvk.Values;            
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            var dictValues = values?.ToDictionary();
            var defWin = Default;
            Width = dictValues.GetValue("Width", defWin.Width);
            Quarter = dictValues.GetValue("Quarter", defWin.Quarter);
            ShadowAngle = dictValues.GetValue("ShadowAngle", defWin.ShadowAngle);
            IsCustomAngle = dictValues.GetValue("IsCustomAngle", defWin.IsCustomAngle);            
        }

        public bool Equals(WindowOptions other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            var res = IsCustomAngle == other.IsCustomAngle &&
                Construction.Equals(other.Construction) &&
                NetLib.MathExt.IsEqual(Width, other.Width) &&
                NetLib.MathExt.IsEqual(Quarter, other.Quarter);
            return res;
        }
        public override int GetHashCode()
        {
            return ShadowAngle.GetHashCode();
        }
    }
}
