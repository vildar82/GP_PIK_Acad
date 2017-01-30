using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Elements;
using PIK_GP_Acad.Elements.Buildings;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчетная область
    /// </summary>
    public class Scope : IDisposable
    {        
        public List<MapBuilding> Buildings { get; set; }        
        public Map Map { get; set; }

        public Scope (List<MapBuilding> items, Map map)
        {   
            Map = map;
            Buildings = items;
        }

        public void InitBuildingContours ()
        {
            foreach (var item in Buildings)
            {
                // Если есть старый контур - удаление и создание нового
                //DisposeBuildingContour(item);
                if (item.Contour == null || item.Contour.IsDisposed)
                {
                    item.InitContour();
                }
            }
        }       

        public void Dispose ()
        {
            if (Buildings == null) return;
            foreach (var item in Buildings)
            {
                item?.Contour?.Dispose();                
            }
            Buildings = null;
        }
    }
}
