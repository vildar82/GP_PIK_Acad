using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using PIK_GP_Acad.Insolation;

namespace PIK_GP_Acad.Model.Insolation.Shadow.Visualization
{
    class Visual
    {
        static List<Drawable> draws;
        public void Show(Map map)
        {
            draws = new List<Drawable>();
            TransientManager tm = TransientManager.CurrentTransientManager;
            var ic = new Autodesk.AutoCAD.Geometry.IntegerCollection();
            foreach (var item in map.Tiles)
            {
                var d = item.CreateVisual();
                draws.Add(d);
                tm.AddTransient(d, TransientDrawingMode.Main, 0, ic);
            }
        }
    }
}
