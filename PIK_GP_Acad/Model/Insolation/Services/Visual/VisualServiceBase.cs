using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;

namespace PIK_GP_Acad.Insolation.Services
{
    public abstract class VisualServiceBase : IVisualService
    {
        private Autodesk.AutoCAD.Geometry.IntegerCollection vps = new Autodesk.AutoCAD.Geometry.IntegerCollection();        
        private bool isOn;
        private List<Drawable> draws;
        protected List<IVisual> visuals;        

        public bool IsOn {
            get { return isOn; }
            set {
                if (value != isOn)
                {
                    isOn = value;
                    Update();                    
                }
            }
        }        

        /// <summary>
        /// Включение/отключение визуализации (без перестроений)
        /// </summary>
        public void Update ()
        {            
            if (visuals != null)
            {
                var tm = TransientManager.CurrentTransientManager;
                
                // Включение визуализации на чертеже
                if (isOn)
                {
                    UpdateDraws();                    
                    foreach (var d in draws)
                    {
                        tm.AddTransient(d, TransientDrawingMode.Main, 0, vps);
                    }
                }
                // Выключение
                else
                {
                    EraseDraws ();
                }
            }
        }

        private void EraseDraws ()
        {
            if (draws != null)
            {
                var tm = TransientManager.CurrentTransientManager;
                foreach (var item in draws)
                {
                    tm.EraseTransient(item, vps);
                    item.Dispose();
                }
            }
        }

        private void UpdateDraws ()
        {
            EraseDraws();
            draws = new List<Drawable>();
            if (visuals == null) return;            
            foreach (var item in visuals)
            {
                var ds = item.CreateVisual();
                draws.AddRange(ds);                
            }
        }        
    }
}
