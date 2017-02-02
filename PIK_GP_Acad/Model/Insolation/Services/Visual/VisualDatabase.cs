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
    public abstract class  VisualDatabase : VisualBase
    {        
        private const string LayerVisualName = "ins_sapr_visuals"; // Слой для отрисовки визуализации при расчете. layerForUser - слой для отрисовки визуализации для пользователя   
        internal List<ObjectId> draws;
        protected Document doc; 

        public VisualDatabase(Document doc, string layerForUser = null) : base(layerForUser)
        {
            this.doc = doc;
        }

        protected override void DrawVisuals(List<Entity> ds)
        {
            draws = new List<ObjectId>();
            if (ds != null && ds.Count != 0)
            {                
                using (doc.LockDocument())
                using (var t = doc.TransactionManager.StartTransaction())
                {
                    var db = doc.Database;
                    var msId = SymbolUtilityServices.GetBlockModelSpaceId(db);
                    var ms = msId.GetObject(OpenMode.ForWrite) as BlockTableRecord;

                    // Слой для визуализации                    
                    var idLayerVisual = GetLayerForVisual(LayerVisualName);

                    foreach (var d in ds)
                    {
                        if (d.Id.IsNull)
                        {
                            if (d.LayerId != idLayerVisual)
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

        protected override void EraseDraws ()
        {
            if (draws != null && draws.Any() && doc != null && !doc.IsDisposed)
            {
                using (new WorkingDatabaseSwitcher(doc.Database))
                {
                    using (doc.LockDocument())
                    using (var t = doc.TransactionManager.StartTransaction())
                    {
                        foreach (var item in draws)
                        {
                            if (!item.IsValidEx()) continue;
                            try
                            {
                                var ent = item.GetObject(OpenMode.ForWrite, false, true) as Entity;
                                // Удаляем, только, если слой остался прежним
                                if (ent.Layer == LayerForUser)
                                {
                                    ent.Erase();
                                }
                            }
                            catch { }
                        }
                        t.Commit();
                    }
                }
            }
        }                                              

        ///// <summary>
        ///// Проверка, это элемент визуализации
        ///// </summary>                
        //public static bool IsVisualElement(Entity ent)
        //{
        //    var res = ent.Layer.Equals(LayerVisualName, StringComparison.OrdinalIgnoreCase);
        //    return res;
        //}
    }
}
