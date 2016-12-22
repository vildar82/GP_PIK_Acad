using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.WPF;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Services;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.Models
{       
    /// <summary>
    /// Конструкция окна
    /// </summary>
    public class WindowConstruction : ModelBase, ITypedDataValues, IEquatable<WindowConstruction>
    {
        public WindowConstruction () { }
        /// <summary>
        /// Тип конструкции 
        /// </summary>
        public string Name { get { return name; } set { name = value; RaisePropertyChanged(); } }
        string name;

        /// <summary>
        /// расстояние от наружной поверхности стены со светопроемом до внутренней поверхности переплета [м]
        /// </summary>
        public double Depth { get { return depth; } set { depth = value; RaisePropertyChanged(); } }
        double depth;

        public static List<WindowConstruction> WindowConstructions { get; set; } = new List<WindowConstruction>() {
            new WindowConstruction() { Name ="Одинарный оконный блок с одним стеклом", Depth = 0.06 },
            new WindowConstruction() { Name ="Одинарный оконный блок с однокамерным стеклопакетом", Depth = 0.09 },
            new WindowConstruction() { Name ="Одинарный оконный блок с двухкамерным стеклопакетом", Depth = 0.095 },
            new WindowConstruction() { Name ="Спаренный оконный блок с двойным остеклением", Depth = 0.11 },
            new WindowConstruction() { Name ="Спаренный оконный блок со стеклом и стеклопакетом ", Depth = 0.13 },
            new WindowConstruction() { Name ="Раздельный оконный блок с двойным остеклением", Depth = 0.155 },
            new WindowConstruction() { Name ="Раздельно-спаренный оконный блок с тройным остеклением", Depth = 0.155 },
            new WindowConstruction() { Name ="Раздельный блок со стеклом и однокамерным стеклопакетом", Depth = 0.205 },
            new WindowConstruction() { Name ="Раздельный оконный блок со стеклом и двухкамерным стеклопакетом", Depth = 0.21 },
            new WindowConstruction() { Name ="Раздельный оконный блок с двумя однокамерными стеклопакетами", Depth = 0.2 },
        };

        public static WindowConstruction Find (string wcName)
        {
            var constr = WindowConstructions.FirstOrDefault(w => w.Name == wcName);
            if (constr == null)
                constr = WindowConstructions[0];
            return constr;
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            var tvk = new TypedValueExtKit();
            tvk.Add("Name", Name);
            tvk.Add("Depth", Depth);
            return tvk.Values;            
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            var dictValues = values?.ToDictionary();
            var constr = WindowConstructions[0];
            Name = dictValues.GetValue("Name", constr.Name);
            Depth = dictValues.GetValue("Depth", constr.Depth);            
        }

        public static WindowConstruction GetStandart (WindowConstruction constr)
        {
            var standConstr = WindowConstruction.WindowConstructions.FirstOrDefault(w => w.Equals(constr));
            if (standConstr == null)
                standConstr = constr;
            return standConstr;
        }

        public bool Equals (WindowConstruction other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Depth == other.Depth && Name == other.Name;
        }

        public override int GetHashCode ()
        {
            return Name.GetHashCode();
        }
    }
}
