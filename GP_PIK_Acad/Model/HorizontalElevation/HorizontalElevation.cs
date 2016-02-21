using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace GP_PIK_Acad.Model.HorizontalElevation
{
   /// <summary>
   /// Изменение уровней горизонталей
   /// </summary>
   public class HorizontalElevation
   {
      Document doc;
      Database db;
      Editor ed;
      
      public HorizontalElevation()
      {
         doc =  Application.DocumentManager.MdiActiveDocument;
         db = doc.Database;
         ed = doc.Editor;
      }   

      /// <summary>
      /// Поочередное изменени уровня выбранной горизонтали на заданный шаг от стартового значения.
      /// </summary>
      public void Stepping()
      {
         FormHorizontalElevation formHorElev = new FormHorizontalElevation(HorizontalElevationOptions.Instance.StartElevation,
                                                       HorizontalElevationOptions.Instance.StepElevation);
         if (Application.ShowModalDialog(formHorElev) == System.Windows.Forms.DialogResult.OK)
         {
            double curElev = formHorElev.StartElevation;
            double stepElev = formHorElev.StepElevation;
            do
            {
               // Запрос выбора горизонтали - полилинии
               var plId = getHorizontal(curElev);
               using (var pl = plId.Open(OpenMode.ForWrite, false, true) as Polyline)
               {
                  if (pl == null)
                  {
                     throw new Exception("Прервано - Не удалось определить выбранный объект.");
                  }
                  pl.Elevation = curElev;
               }
               curElev += stepElev;
            } while (true);
         }
      }

      private ObjectId getHorizontal(double curElev)
      {
         var prOpt = new PromptEntityOptions($"\nВыберите горизонталь для установки ей уровня {curElev}:");
         prOpt.SetRejectMessage("Можно выбрать только полилинию");
         prOpt.AddAllowedClass(typeof(Polyline), true);
         prOpt.AllowNone = false;
         prOpt.AllowObjectOnLockedLayer = true;         

         var prRes = ed.GetEntity(prOpt);
         if (prRes.Status == PromptStatus.OK)
         {
            return prRes.ObjectId;
         }
         throw new Exception("Отменено пользователем.");
      }
   }
}
