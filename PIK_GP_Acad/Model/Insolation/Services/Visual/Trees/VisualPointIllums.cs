using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Инсоляционные зоны точки
    /// </summary>
    public class VisualPointIllums : VisualTransient
    {
        public InsPoint InsPoint { get; set; }

        public VisualPointIllums(InsPoint insPoint)
        {
            InsPoint = insPoint;
        }

        public override List<Entity> CreateVisual ()
        {
            var draws = new List<Entity>();

            if (InsPoint.Illums != null)
            {
                var transp = InsPoint?.Model?.Tree?.TreeOptions?.Transparence ?? 60;
                foreach (var item in InsPoint.Illums)
                {
                    draws.AddRange(item.CreateVisual(transp));
                }
            }
            return draws;
        }       
    }
}
