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
    public abstract class VisualTransient : IVisualService
    {
        private Autodesk.AutoCAD.Geometry.IntegerCollection vps = new Autodesk.AutoCAD.Geometry.IntegerCollection();
        private bool isOn;
        private List<Entity> draws;
        
        public abstract List<Entity> CreateVisual ();

        public bool VisualIsOn {
            get { return isOn; }
            set {
                isOn = value;
                VisualUpdate();
            }
        }

        /// <summary>
        /// Включение/отключение визуализации (без перестроений)
        /// </summary>
        public void VisualUpdate ()
        {                       
            // Включение визуализации на чертеже
            if (isOn)
            {
                UpdateDraws();
                if (draws != null)
                {                    
                    var tm = TransientManager.CurrentTransientManager;
                    foreach (var d in draws)
                    {
                        tm.AddTransient(d, TransientDrawingMode.Main, 0, vps);
                    }
                }
            }
            // Выключение
            else
            {
                EraseDraws();
            }
        }

        private void EraseDraws ()
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

        private void UpdateDraws ()
        {
            EraseDraws();
            draws = CreateVisual();            
        }

        public virtual void VisualsDelete ()
        {
            EraseDraws();
        }

        public virtual void Dispose ()
        {
            if (draws != null)
            {
                EraseDraws();                
            }
        }

        public void DrawForUser()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            var visDbAny = new VisualDatabaseAny(doc);
            visDbAny.AddVisual(this);            
            visDbAny.Draw();
        }
    }
}
