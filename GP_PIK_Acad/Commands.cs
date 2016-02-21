using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using GP_PIK_Acad.Model.HorizontalElevation;

[assembly: CommandClass(typeof(GP_PIK_Acad.Commands))]

namespace GP_PIK_Acad
{
   public class Commands
   {
      /// <summary>
      /// Изменение уровней горизонталей
      /// </summary>
      [CommandMethod("PIK", "GP-HorizontalElevationStep", CommandFlags.Modal)]
      public void GP_HorizontalElevationStep()
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         try
         {
            Inspector.Clear();
            HorizontalElevation horElev = new HorizontalElevation();
            horElev.Stepping();

            if (Inspector.HasErrors)
            {
               Inspector.Show();
            }
         }
         catch (System.Exception ex)
         {
            if (!ex.Message.Contains("Отменено пользователем"))
            {
               Logger.Log.Error(ex, "GP-HorizontalElevationStep");
            }
            doc.Editor.WriteMessage(ex.Message);
         }         
      }
   }
}
