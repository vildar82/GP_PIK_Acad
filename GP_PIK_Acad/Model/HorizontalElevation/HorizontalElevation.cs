using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;

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
      TransientManager tm;
      List<DBText> tempTexts;

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

            using (var t = db.TransactionManager.StartTransaction())
            {
               tempTexts = new List<DBText>();
               tm = TransientManager.CurrentTransientManager;
               bool isContinue = true;
               do
               {                  
                  Point3d ptPicked;
                  var plId = getHorizontal(curElev, out ptPicked);
                  if (plId.IsNull)
                  {
                     ed.WriteMessage("\nПрервано пользоваателем.");
                     isContinue = false;
                  }
                  else
                  {
                     var pl = plId.GetObject(OpenMode.ForWrite, false, true) as Autodesk.AutoCAD.DatabaseServices.Polyline;
                     if (pl == null)
                     {
                        ed.WriteMessage("\nПрервано - Не удалось определить выбранный объект.");
                        isContinue = false;
                     }
                     else
                     {
                        pl.Elevation = curElev;
                        // Текст назначенного уровня для полилинии
                        addText(curElev, ptPicked);
                        // Изменение текущего уровня на шаг
                        curElev += stepElev;
                     }                     
                  }                  
               } while (isContinue);
               ClearTransientGraphics();
               t.Commit();
            }
         }
      }

      private void addText(double level, Point3d pt)
      {         
         DBText text = new DBText();
         text.Position = pt;                  
         text.TextString = level.ToString();
         //text.Justify = AttachmentPoint.MiddleCenter;
         //text.AlignmentPoint = pt;
         text.Height = ed.GetCurrentView().Height * HorizontalElevationOptions.Instance.TextHeight;
         //text.ColorIndex = 11;//Color.FromColor(HorizontalElevationOptions.Instance.TextColor);
         IntegerCollection intCol = new IntegerCollection();
         tm.AddTransient(text, TransientDrawingMode.Main, 0, intCol);
         tempTexts.Add(text);
      }

      private ObjectId getHorizontal(double curElev, out Point3d ptPicked)
      {
         var prOpt = new PromptEntityOptions($"\nВыберите горизонталь для установки ей уровня {curElev}:");
         prOpt.SetRejectMessage("Можно выбрать только полилинию");
         prOpt.AddAllowedClass(typeof(Autodesk.AutoCAD.DatabaseServices.Polyline), true);
         prOpt.AllowNone = false;
         prOpt.AllowObjectOnLockedLayer = true;         

         var prRes = ed.GetEntity(prOpt);
         if (prRes.Status == PromptStatus.OK)
         {
            ptPicked = prRes.PickedPoint;
            return prRes.ObjectId;
         }
         ptPicked = Point3d.Origin;
         return ObjectId.Null;         
      }

      // Erases any transient graphics
      private void ClearTransientGraphics()
      {         
         IntegerCollection intCol = new IntegerCollection();
         if (tempTexts != null)
         {
            foreach (DBObject transient in tempTexts)
            {
               tm.EraseTransient(
                                   transient,
                                   intCol
                                   );
               transient.Dispose();
            }
            tempTexts.Clear();
         }         
      }
   }
}
