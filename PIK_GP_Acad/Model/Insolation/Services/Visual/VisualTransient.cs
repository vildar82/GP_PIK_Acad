using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Визуализация графики - через TransientManager
    /// </summary>
    public abstract class VisualTransient : VisualBase
    {
        public static Autodesk.AutoCAD.Geometry.IntegerCollection vps = new Autodesk.AutoCAD.Geometry.IntegerCollection();        
        protected List<Entity> draws;

        public VisualTransient(string layer = null) : base(layer)
        {

        }

        public virtual List<Entity> GetDraws()
        {
            return draws;
        }

        /// <summary>
        /// Включение/отключение визуализации (без перестроений)
        /// </summary>
        protected override void DrawVisuals(List<Entity> draws)
        {
            this.draws = draws;
            if (draws != null)
            {
                var tm = TransientManager.CurrentTransientManager;
                foreach (var d in draws)
                {
                    tm.AddTransient(d, TransientDrawingMode.Main, 0, vps);
                }
            }
        }

        protected override void EraseDraws ()
        {
            if (draws == null || draws.Count == 0) return;
            var tm = TransientManager.CurrentTransientManager;
            foreach (var item in draws)
            {
                tm.EraseTransient(item, vps);
                item.Dispose();
            }
            draws = null;
        }        
    }
}
