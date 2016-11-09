using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Выбор площадки
    /// </summary>
    public class SelectPlace
    {
        public ObjectId Select()
        {
            // Выбор полилинии
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var selOpt = new PromptEntityOptions("\nВыбор полилинии площадки:");
            selOpt.SetRejectMessage("\nТолько полилинию.");
            selOpt.AddAllowedClass(typeof(Polyline), true);            
            var selRes = ed.GetEntity(selOpt);            
            if (selRes.Status != PromptStatus.OK)
            {
                return ObjectId.Null;
            }
            return selRes.ObjectId;
        }
    }
}
