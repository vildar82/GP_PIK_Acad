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
    /// Визуализация графики в чертеже (в базе чертежа)
    /// </summary>
    public abstract class  VisualDatabase : IVisualService
    {
        private Autodesk.AutoCAD.Geometry.IntegerCollection vps = new Autodesk.AutoCAD.Geometry.IntegerCollection();
        private bool isOn;
        private List<ObjectId> draws;
        private Document doc;

        public abstract List<Entity> CreateVisual ();

        public VisualDatabase (Document doc)
        {
            this.doc = doc;
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
                    using (doc.LockDocument())
                    using (var t = doc.TransactionManager.StartTransaction())
                    {
                        var db = doc.Database;
                        var msId = SymbolUtilityServices.GetBlockModelSpaceId(db);
                        var ms = msId.GetObject(OpenMode.ForWrite) as BlockTableRecord;

                        foreach (var d in ds)
                        {
                            if (d.Id.IsNull)
                            {
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
            if (draws != null)
            {
                using (doc.LockDocument())
                using (var t = doc.TransactionManager.StartTransaction())
                {
                    foreach (var item in draws)
                    {
                        if (item.IsNull || item.IsErased) continue;
                        var ent = item.GetObject(OpenMode.ForWrite, false, true);
                        ent.Erase();
                    }
                    t.Commit();
                }
            }
        }              

        public void VisualsDelete ()
        {
            EraseDraws();
        }
    }
}
