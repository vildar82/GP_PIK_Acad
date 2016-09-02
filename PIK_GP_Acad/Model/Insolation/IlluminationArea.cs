using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation
{
    /// <summary>
    /// Зона освещенности - контур освещенности для заданной точки
    /// 
    /// </summary>
    public class IlluminationArea
    {
        private IInsolationService insService;
        public Polyline Low { get; set; }
        public Polyline Medium { get; set; }
        public Polyline Hight { get; set; }
        /// <summary>
        /// Угол начала освещенности
        /// </summary>
        public double AngleStart { get; set; }
        /// <summary>
        /// Угол конца освещенности
        /// </summary>
        public double AngleEnd { get; set; }

        public IlluminationArea (IInsolationService insService, double angleStart, double angleEnd)
        {
            this.insService = insService;
            AngleStart = angleStart;
            AngleEnd = angleEnd;
        }

        /// <summary>
        /// Построение контура освещенности
        /// </summary>        
        public void Create (BlockTableRecord space)
        {
            Transaction t = space.Database.TransactionManager.TopTransaction;
            createPl(Low, insService.Options.ColorLow, space, t);
            createPl(Medium, insService.Options.ColorMedium, space, t);
            createPl(Hight, insService.Options.ColorHight, space, t);
        }

        private void createPl(Polyline pl, Color color, BlockTableRecord cs, Transaction t)
        {            
            pl.Color = color;
            pl.Transparency = new Transparency(insService.Options.Transparence);
            cs.AppendEntity(pl);
            t.AddNewlyCreatedDBObject(pl, true);
            var h = AcadLib.Hatches.HatchExt.CreateAssociativeHatch(pl, cs, t);
            if (h != null)
            {
                h.Color = color;
                h.Transparency = new Transparency(insService.Options.Transparence);
            }
        }
    }
}
