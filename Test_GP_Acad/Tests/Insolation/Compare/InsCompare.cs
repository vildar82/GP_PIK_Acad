using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_GP_Acad.Tests.Insolation.Compare
{
    public class InsCompare
    {
        private string file1;
        private string file2;

        public InsCompare(string file1, string file2)
        {
            this.file1 = file1;
            this.file2 = file2;
        }

        public void Compare()
        {
            // Объекты инсоляции correct
            var ins1 = new InsObjects(); GetInsObjects(file1, a=> ins1.Add(a));
            var ins2 = new InsObjects(); GetInsObjects(file2, a => ins2.Add(a));

            ins1.Difference(ins2, $"Отличие объекта в файле {Path.GetFileNameWithoutExtension(file1)}",System.Drawing.SystemIcons.Exclamation);
            ins2.Difference(ins1, $"Отличие объекта в файле {Path.GetFileNameWithoutExtension(file2)}",System.Drawing.SystemIcons.Error);
        }

        public static void GetInsObjects(string file, Action<Entity> action)
        {            
            var doc = GetDocumentOrOpen(file);
            var dbIns = doc.Database;
            using (doc.LockDocument())
            using (var t = dbIns.TransactionManager.StartTransaction())
            {
                var ms = SymbolUtilityServices.GetBlockModelSpaceId(dbIns).GetObject(OpenMode.ForRead) as BlockTableRecord;
                foreach (var idEnt in ms)
                {
                    if (!idEnt.IsValidEx()) continue;

                    var ent = idEnt.GetObject(OpenMode.ForRead) as Entity;

                    if (!InsObjects.IsInsObject(ent) ||
                        ent is Circle || ent is DBPoint || ent is DBText || ent is MText || ent is Dimension)
                        continue;

                    action(ent);
                }
                t.Commit();
            }           
        }

        public static Document GetDocumentOrOpen(string file)
        {
            foreach (Document item in Application.DocumentManager)
            {
                if (string.Equals(file, item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    Application.DocumentManager.MdiActiveDocument = item;
                    return item;
                }
            }
            // Открытин документа
            var doc = Application.DocumentManager.Open(file, false);
            Application.DocumentManager.MdiActiveDocument = doc;
            return doc;
        }
    }
}
