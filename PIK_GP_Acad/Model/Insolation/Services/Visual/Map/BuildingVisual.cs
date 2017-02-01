using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    public class BuildingVisual : VisualTransient
    {
        private MapBuilding building;
        public BuildingVisual(MapBuilding building)
        {
            this.building = building;
            isOn = true; // Включена визуализация по умолчанию
        }

        public bool IsVisualizedInFront { get; set; }

        public override List<Entity> CreateVisual()
        {
            if (IsVisualizedInFront == true)
                return null;
            return building.CreateVisual();                        
        }

        public override void VisualUpdate()
        {
            if (IsVisualizedInFront)
                return;
            base.VisualUpdate();
        }
    }
}
