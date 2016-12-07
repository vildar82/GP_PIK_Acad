using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Layers;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using AcadLib;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Визуализация графики в чертеже (в базе чертежа)
    /// </summary>
    public abstract class  VisualDatabase : IVisualService
    {        
        private bool isOn;
        private List<ObjectId> draws;
        private LayerInfo lay;

        public Document Doc { get; set; }
        public string LayerVisual { get; set; } = "sapr_ins_visuals";

        public abstract List<Entity> CreateVisual ();

        public VisualDatabase (Document doc)
        {
            this.Doc = doc;
        }

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
            EraseDraws();
            // Включение визуализации на чертеже
            if (isOn)
            {
                draws = new List<ObjectId>();
                var ds = CreateVisual();
                if (ds != null && ds.Count != 0)
                {
                    using (Doc.LockDocument())
                    using (var t = Doc.TransactionManager.StartTransaction())
                    {
                        var db = Doc.Database;
                        var msId = SymbolUtilityServices.GetBlockModelSpaceId(db);
                        var ms = msId.GetObject(OpenMode.ForWrite) as BlockTableRecord;

                        // Слой для визуализации
                        var idLayerVisual = GetLayerForVisual(db);

                        foreach (var d in ds)
                        {
                            if (d.Id.IsNull)
                            {
                                d.LayerId = idLayerVisual;                
                                ms.AppendEntity(d);
                                t.AddNewlyCreatedDBObject(d, true);
                            }
                            draws.Add(d.Id);
                        }
                        t.Commit();
                    }
                }
            }
        }

        private void EraseDraws ()
        {
            if (draws != null && Doc != null && !Doc.IsDisposed)
            {
                using (new WorkingDatabaseSwitcher(Doc.Database))
                {
                    using (Doc.LockDocument())
                    using (var t = Doc.TransactionManager.StartTransaction())
                    {
                        foreach (var item in draws)
                        {
                            if (!item.IsValidEx()) continue;
                            try
                            {
                                var ent = item.GetObject(OpenMode.ForWrite, false, true);
                                ent.Erase();                            
                            }
                            catch { }
                        }
                        t.Commit();
                    }
                }
            }
        }              

        public void VisualsDelete ()
        {
            try
            {
                EraseDraws();
            }
            catch { }
        }

        private ObjectId GetLayerForVisual (Database db)
        {
            if (lay == null)
            {
                lay = new LayerInfo(LayerVisual);
            }
            return lay.CheckLayerState();            
        }

        public virtual void Dispose ()
        {
            EraseDraws();
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
