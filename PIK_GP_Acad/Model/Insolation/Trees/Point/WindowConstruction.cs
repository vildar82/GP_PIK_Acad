using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.WPF;

namespace PIK_GP_Acad.Insolation.Trees
{    
    public class WindowConstruction
    {
        /// <summary>
        /// Тип конструкции 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// расстояние от наружной поверхности стены со светопроемом до внутренней поверхности переплета [м]
        /// </summary>
        public double Depth { get; set; }

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
    }
}
