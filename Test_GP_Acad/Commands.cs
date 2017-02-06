using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Test_GP_Acad.Tests.Insolation;


namespace Test_GP_Acad
{   
    public class Commands
    {
        [CommandMethod(nameof(TestInsService))]
        public void TestInsService()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            var testIns = new TestInsolationService();
            testIns.CreateInsModelForDoc(doc);
        }
    }
}
