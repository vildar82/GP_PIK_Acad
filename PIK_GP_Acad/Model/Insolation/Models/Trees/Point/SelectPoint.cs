using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        InsModel model;
        Document doc;
        Editor ed;        
        Map map;
        /// <summary>
        /// Выбор новой точки
        /// </summary>        
        public InsPoint SelectNewPoint (InsModel model)
        {
            doc = model.Doc;
            ed = doc.Editor;
            this.model = model;
            map = model.Map;

            InsPoint p = new InsPoint(model);            
            // Запрос точки
            InsBuilding building;
            var pt = PromptSelectPointOnScreen(out building);

            p.Point = pt;
            p.Building = building;

            // Окно настроек расчетной точки (парметры окна, здания, высота точки)
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

        private Point3d PromptSelectPointOnScreen ( out InsBuilding building)
        {
            var pt = ed.GetPointWCS("\nВыбор расчетной точки (на внешней стене здания):");
            // Проверка точки
            building = map.GetBuildingInPoint(pt);
            if (building == null)
            {
                ed.WriteMessage($"\nОшибка. Для указанной точки не определено здание! Повторите...");
                pt = PromptSelectPointOnScreen(out building);
            }
            // Корректировка расчетной точки
            if (!CorrectCalcPoint(ref pt, building))
            {
                ed.WriteMessage($"\nОшибка. Укажите точку точнее (на грани контура полилинии здания)! Повторите...");
                pt = PromptSelectPointOnScreen(out building);
            }
            // Нет ли уже точки в этом месте
            if (model.Tree.HasPoint(pt))
            {
                ed.WriteMessage($"\nОшибка. Указанная точка уже включена в расчет! Повторите...");
                pt = PromptSelectPointOnScreen(out building);
            }
            return pt;
        }

        private bool CorrectCalcPoint (ref Point3d pt, InsBuilding building)
        {
            bool res;
            // Корректировка точки
            Point3d correctPt;
            using (var t = doc.TransactionManager.StartTransaction())
            {
                building.InitContour();
                correctPt = building.Contour.GetClosestPointTo(pt, true);
                t.Commit();
            }
            
            if ((pt - correctPt).Length < 1)
            {
                // Точка достаточно близко к контуру - поправка точки и ОК.
                pt = correctPt;
                res = true;
            }
            else
            {
                // Точка далеко от контура - не пойдет.
                res = false;
            }
            return res;
        }
    }
}
