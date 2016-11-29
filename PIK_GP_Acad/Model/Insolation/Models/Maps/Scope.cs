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
        Extents3d ext;
        public List<MapBuilding> Buildings { get; set; }        
        public Map Map { get; set; }

        public Scope (Extents3d ext, List<MapBuilding> items, Map map)
        {            
            this.ext = ext;
            Map = map;
            Buildings = items;
        }

        public void InitContour ()
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

        private static void DisposeBuildingContour (MapBuilding item)
        {
            if (item.Contour != null && !item.Contour.IsDisposed)
            {
                item.Contour.Dispose();
            }
        }

        public void Dispose ()
        {
            if (Buildings == null) return;
            foreach (var item in Buildings)
            {
                DisposeBuildingContour(item);                
            }
            Buildings = null;
        }
    }
}
