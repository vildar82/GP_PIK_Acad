using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Exceptions;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Catel.IoC;
using Catel.Services;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.UI;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Выбор расчетной точки на чертеже
    /// </summary>
    public class SelectPoint
    {
        Document doc;
        /// <summary>
        /// Выбор новой точки
        /// </summary>        
        public InsPoint SelectNewPoint (InsModel model)
        {
            doc = model.Doc;
            InsPoint p = new InsPoint(model);
            Editor ed = doc.Editor;
            // Запрос точки
            InsBuilding building;
            var pt = PromptSelectPointOnScreen(ed, model.Map, out building);

            p.Point = pt;
            p.Building = building;

            // Запрос настроек расчетной точки
            var pViewModel = new InsPointViewModel(p);
            var uiVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            if (uiVisualizerService.ShowDialog(pViewModel) == true)
            {
                return p;
            }
            else
            {
                return null;
            }            
        }

        private Point3d PromptSelectPointOnScreen (Editor ed, Map map, out InsBuilding building)
        {
            var pt = ed.GetPointWCS("\nВыбор расчетной точки (на внешней стене здания):");
            // Проверка точки
            building = map.GetBuildingInPoint(pt);
            if (building == null)
            {
                ed.WriteMessage($"\nОшибка. Для указанной точки не определено здание! Необходимо повторить выбор точки.");
                PromptSelectPointOnScreen(ed, map, out building);
            }
            // Корректировка расчетной точки
            if (!CorrectCalcPoint(ref pt, building))
            {
                ed.WriteMessage($"\nОшибка. Укажите точку точнее (на грани контура полилинии здания)!");
                PromptSelectPointOnScreen(ed, map, out building);
            }
            return pt;
        }

        private bool CorrectCalcPoint (ref Point3d pt, InsBuilding building)
        {
            // Корректировка точки
            Point3d correctPt;
            using (var t = doc.TransactionManager.StartTransaction())
            {
                building.InitContour();
                correctPt = building.Contour.GetClosestPointTo(pt, true);
                t.Commit();
            }
            
            var res = (pt - correctPt).Length < 5;
            return res;
        }
    }
}
