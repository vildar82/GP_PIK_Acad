using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib.Layers;
using Autodesk.AutoCAD.GraphicsInterface;

namespace PIK_GP_Acad.Insolation.Services
{
    public class VisualDatabaseAny: VisualDatabase
    {        
        private List<IVisualService> visualsServices = new List<IVisualService>();

        public VisualDatabaseAny (Document doc, string layer = null) : base(doc, layer)
        {            
        }

        /// <summary>
        /// Добавление визуализации
        /// </summary>
        /// <param name="visual"></param>
        public void AddVisual (IVisualService visual)
        {
            if (visual != null)
            {
                visualsServices.Add(visual);                                
            }
        }

        public override List<Entity> CreateVisual()
        {
            return null;// draws;
        }

        /// <summary>
        /// Рисование визуализации на чертеже
        /// </summary>
        private void Draw ()
        {            
            using (doc.LockDocument())
            using (var t = doc.TransactionManager.StartTransaction())
            {
                var draws = new List<Entity>();
                foreach (var visual in visualsServices)
                {
                    var layId = GetLayerForVisual(visual.LayerForUser);
                    if (visual is VisualDatabase)
                    {
                        // Только перенести отрисованные объекты визуализации на нужный слой
                        var vdb = (VisualDatabase)visual;
                        foreach (var item in vdb.draws)
                        {
                            var ent = item.GetObject(OpenMode.ForWrite) as Entity;
                            if (ent.Layer != vdb.LayerForUser)
                                ent.LayerId = layId;
                        }
                    }
                    else if (visual is VisualTransient)
                    {
                        var vt = (VisualTransient)visual;
                        // Отрисовка объектов в базе на нужном слое и удаление временной графики
                        var items = vt.GetDraws();
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                if (item.Layer != visual.LayerForUser)
                                    item.LayerId = layId;
                                draws.Add(item);
                            }
                        }
                    }
                }

                if (draws.Any())
                {
                    var ms = SymbolUtilityServices.GetBlockModelSpaceId(doc.Database).GetObject(OpenMode.ForWrite) as BlockTableRecord;
                    var tm = TransientManager.CurrentTransientManager;
                    foreach (var item in draws)
                    {
                        tm.EraseTransient(item, VisualTransient.vps);
                        ms.AppendEntity(item);
                        t.AddNewlyCreatedDBObject(item, true);
                    }
                }                

                t.Commit();
            }
        }

        //public override void Dispose()
        //{
        //    if (draws != null)
        //    {
        //        foreach (var item in draws)
        //        {
        //            item.Dispose();
        //        }
        //    }
        //    base.Dispose();
        //}

        public static void DrawVisualsForUser(params IVisualService[] visuals)
        {
            DrawVisualsForUser(visuals);
        }

        public static void DrawVisualsForUser(List<IVisualService> visuals)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;            
            using (var visDbAny = new VisualDatabaseAny(doc))
            {
                foreach (var visual in visuals)
                {
                    visDbAny.AddVisual(visual);
                }
                visDbAny.Draw();
            }
        }
    }
}
