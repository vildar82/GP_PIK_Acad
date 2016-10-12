using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.GraphicsInterface;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Инсоляционные зоны точки
    /// </summary>
    public class VisualPointIllums : VisualServiceBase
    {
        public InsPoint InsPoint { get; set; }

        public VisualPointIllums(InsPoint insPoint)
        {
            InsPoint = insPoint;
        }

        public override List<Drawable> CreateVisual ()
        {
            List<Drawable> draws = new List<Drawable>();

            if (InsPoint.Illums != null)
            {
                foreach (var item in InsPoint.Illums)
                {
                    draws.AddRange(item.CreateVisual());
                }
            }
            return draws;
        }       
    }
}
