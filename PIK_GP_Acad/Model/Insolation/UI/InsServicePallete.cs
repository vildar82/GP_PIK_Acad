using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsServicePallete : PaletteSet
    {        
        public InsServicePallete(InsView control) : base("Инсоляция", new Guid("E0E995F6-3D01-4D19-A888-BD167F1B9B5E"))
        {
            Icon = Properties.Resources.Sun;
            MinimumSize = new System.Drawing.Size(50, 50);
            AddVisual("", control);            
        }
    }
}
