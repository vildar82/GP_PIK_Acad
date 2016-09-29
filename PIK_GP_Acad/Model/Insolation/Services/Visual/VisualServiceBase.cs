using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.GraphicsInterface;

namespace PIK_GP_Acad.Insolation.Services
{
    class VisualServiceBase : IVisualService
    {
        private bool isOn;
        private List<Drawable> draws;
        protected List<IVisual> visuals;
        protected IVisualOptions visualOPtions;

        public void Off ()
        {
            isOn = false;
            Update();
        }

        public void On ()
        {
            isOn = true;
            Update();
        }       

        protected void Update()
        {
            draws = null;
            if (isOn && visuals != null && visuals.Any())
            {
                TransientManager tm = TransientManager.CurrentTransientManager;
                var ic = new Autodesk.AutoCAD.Geometry.IntegerCollection();
                foreach (var item in visuals)
                {
                    var ds = item.CreateVisual(visualOPtions);
                    draws.AddRange(ds);
                    foreach (var d in ds)
                    {
                        tm.AddTransient(d, TransientDrawingMode.Main, 0, ic);
                    }                    
                }
            }
        }

        //void ClearTransientGraphics ()
        //{
        //    TransientManager tm
        //            = TransientManager.CurrentTransientManager;
        //    IntegerCollection intCol = new IntegerCollection();
        //    if (_markers != null)
        //    {
        //        foreach (DBObject marker in _markers)
        //        {
        //            tm.EraseTransient(
        //                                marker,
        //                                intCol
        //                             );
        //            marker.Dispose();
        //        }
        //    }
        //}
    }
}
