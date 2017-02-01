using AcadLib;
using AcadLib.Layers;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Services
{
    public abstract class VisualBase : IVisualService
    {        
        protected bool isOn;                
        public string LayerForUser { get; set; }         

        public VisualBase(string layer = null)
        {
            LayerForUser = layer ?? SymbolUtilityServices.LayerZeroName;
        }

        public abstract List<Entity> CreateVisual();        
        protected abstract void DrawVisuals(List<Entity> draws);
        protected abstract void EraseDraws();

        public bool VisualIsOn {
            get { return isOn; }
            set {
                isOn = value;
                VisualUpdate();
            }
        }

        public virtual void VisualUpdate()
        {
            EraseDraws();
            // Включение визуализации на чертеже
            if (isOn)
            {
                DrawVisuals(CreateVisual());
            }
        }

        public virtual void VisualsDelete()
        {
            try
            {
                EraseDraws();
            }
            catch { }
        }

        protected ObjectId GetLayerForVisual(string layer)
        {
            var lay = new LayerInfo(layer ?? SymbolUtilityServices.LayerZeroName);
            return lay.CheckLayerState();
        }

        public virtual void Dispose()
        {
            EraseDraws();
        }

        //public void DrawForUser()
        //{
        //    var doc = Application.DocumentManager.MdiActiveDocument;
        //    if (doc == null) return;
        //    using (doc.LockDocument())
        //    using (var visDbAny = new VisualDatabaseAny(doc, LayerForUser))
        //    {
        //        visDbAny.AddVisual(this);
        //        visDbAny.Draw();
        //    }
        //}        
    }
}
