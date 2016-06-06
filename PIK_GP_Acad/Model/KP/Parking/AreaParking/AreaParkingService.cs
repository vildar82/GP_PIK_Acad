using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace PIK_GP_Acad.KP.Parking
{
    /// <summary>
    /// Расчет парковок свободной площади
    /// </summary>
    public class AreaParkingService
    {        
        public Document Doc { get; set; }
        public Database Db { get; set; }
        public Editor Ed { get; set; }

        /// <summary>
        /// Расчет машиномест парковки
        /// </summary>
        public void Calc()
        {
            // Выбор полилинии парковки
            Doc = Application.DocumentManager.MdiActiveDocument;
            Db = Doc.Database;
            Ed = Doc.Editor;

            var selOpt = new PromptEntityOptions("\nВыбор полилинии парковки:");
            selOpt.SetRejectMessage("\nМожно выбрать только полилинию.");
            selOpt.AddAllowedClass(typeof(Polyline), true);            
            var sel = Ed.GetEntity(selOpt);
            if (sel.Status != PromptStatus.OK)
            {
                return;
            }
            AreaParking parking = new AreaParking(sel.ObjectId, this);

            // Диалоговое окно расчета парковки.       
            FormAreaParking form = new FormAreaParking(parking);
            Application.ShowModalDialog(form);
        }
    }
}
