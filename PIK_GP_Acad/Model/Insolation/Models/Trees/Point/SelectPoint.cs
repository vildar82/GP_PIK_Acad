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

            // Запрос точки            
            MapBuilding building;
            var pt = PromptSelectPointOnScreen(out building);

            var p = new InsPoint(model, pt);
            p.Building = building;           
            // Окно настроек расчетной точки (парметры окна, здания, высота точки)
            var vm = new InsPointViewModel(p);            
            if (InsService.ShowDialog(vm) == true)
            {
                return p;
            }
            else
            {
                return null;
            }            
        }

        private Point3d PromptSelectPointOnScreen (out MapBuilding building)
        {
            var pt = ed.GetPointWCS("\nВыбор расчетной точки (на внешней стене здания):");
            // Проверка точки
            building = InsPointBase.DefineBuilding(ref pt, model);
            if (building == null)
            {
                ed.WriteMessage($"\nОшибка. Здание не определено. Укажите точку на внешнем контуре здания...");
                pt = PromptSelectPointOnScreen(out building);
            }
            else
            {
                // Нет ли уже точки в этом месте
                if (model.Tree.HasPoint(pt))
                {
                    ed.WriteMessage($"\nОшибка. Уже есть расчетная точка в этом месте. Укажите другую точку.");
                    pt = PromptSelectPointOnScreen(out building);
                }                
            }

            return pt;
        }       
    }
}
