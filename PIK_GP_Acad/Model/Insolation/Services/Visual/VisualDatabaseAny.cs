using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Services
{
    public class VisualDatabaseAny: VisualDatabase
    {
        private List<Entity> draws = new List<Entity> ();


        public VisualDatabaseAny (Document doc) : base(doc)
        {
            LayerVisual = SymbolUtilityServices.LayerZeroName;
        }

        /// <summary>
        /// Добавление визуализации
        /// </summary>
        /// <param name="visual"></param>
        public void AddVisual (IVisualService visual)
        {
            if (visual != null)
            {
                var ds = visual.CreateVisual();
                if (ds!= null && ds.Any())
                {
                    draws.AddRange(ds);
                }                
            }
        }

        public override List<Entity> CreateVisual ()
        {
            return draws;
        }

        /// <summary>
        /// Рисование визуализации на чертеже
        /// </summary>
        public void Draw ()
        {
            base.VisualIsOn = true;
        }
    }
}
