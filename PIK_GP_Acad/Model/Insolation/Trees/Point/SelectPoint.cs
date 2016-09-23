using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Exceptions;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation.Trees
{
    /// <summary>
    /// Выбор расчетной точки на чертеже
    /// </summary>
    public class SelectPoint
    {        
        /// <summary>
        /// Выбор новой точки
        /// </summary>        
        public IInsPoint SelectNewPoint (IMap map)
        {
            IInsPoint p= null;

            Editor ed = map.Doc.Editor;
            // Запрос точки
            InsBuilding building;
            var pt = PromptSelectPointOnScreen(ed, map, out building);

            // Запрос настроек расчетной точки

            return p;
        }

        private Point3d PromptSelectPointOnScreen (Editor ed, IMap map, out InsBuilding building)
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
            var correctPt = building.Contour.GetClosestPointTo(pt, true);
            var res = (pt - correctPt).Length > 5;
            return res;
        }
    }
}
