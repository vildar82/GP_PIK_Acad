using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Models;
using PIK_GP_Acad.Insolation.Services;
using System;
using System.Diagnostics;
using System.Linq;

namespace Test_GP_Acad.Tests.Insolation.Compare
{
    public class InsCalc
    {
        private string calcFile;

        public InsCalc(string calcFile)
        {
            this.calcFile = calcFile;
        }

        /// <summary>
        /// Расчет инсоляции в файле
        /// </summary>
        public void Calc()
        {
            var docIns = InsCompare.GetDocumentOrOpen(calcFile);            
            var dbIns = docIns.Database;

            // Очистка инс объектов
            InsCompare.GetInsObjects(calcFile, a =>
            {
                a.UpgradeOpen();
                a.Erase();
                Debug.WriteLine($"Erase - {a}");
            });

            using (docIns.LockDocument())
            using (var t = dbIns.TransactionManager.StartTransaction())
            {
                //InsService
                var insModel = InsModel.LoadIns(docIns);
                insModel.Tree.IsVisualTreeOn = true;
                insModel.Tree.IsVisualIllumsOn = true;
                insModel.Front.Groups?.ToList().ForEach(g => g.IsVisualFrontOn = true);
                insModel.Place.Places?.ToList().ForEach(p => p.IsVisualPlaceOn = true);
                insModel.Place.IsEnableCalc = true;
                                
                insModel.Update(docIns);

                // Визуализация на чертеже всех расчетов
                insModel.Tree.DrawVisuals();
                insModel.Front.DrawVisuals();
                insModel.Place.DrawVisuals();

                t.Commit();
            }            
        }
    }
}