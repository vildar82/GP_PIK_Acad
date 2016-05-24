using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace PIK_GP_Acad.Utils
{
    public static class PolylinePoints
    {
        public static void CreatePoints()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            var selOpt = new PromptEntityOptions("\nВыбор полилинии:");
            selOpt.SetRejectMessage("\nТолько полилинию");
            selOpt.AddAllowedClass(typeof(Curve), false);
            selOpt.AllowObjectOnLockedLayer = true;            
            var sel = ed.GetEntity(selOpt);
            if (sel.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nПрервано пользователем.");
                return;
            }

            using (var t = db.TransactionManager.StartTransaction())
            {
                var cs = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;

                var curve = sel.ObjectId.GetObject(OpenMode.ForRead, false, true) as Curve;

                for (int i = 0; i <= curve.EndParam; i++)
                {
                    var pt = curve.GetPointAtParameter(i);
                    DBPoint dbPoint = new DBPoint(pt);
                    cs.AppendEntity(dbPoint);
                    t.AddNewlyCreatedDBObject(dbPoint, true);
                }

                t.Commit();
            }
        }
    }
}
