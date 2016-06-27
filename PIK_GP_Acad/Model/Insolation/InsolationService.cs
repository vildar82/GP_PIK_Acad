using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Constructions;
using PIK_GP_Acad.Insolation.SunlightRule;

namespace PIK_GP_Acad.Insolation
{
    /// <summary>
    /// Инсоляция - расчет инсоляции в указанных точках
    /// </summary>
    public class InsolationService
    {
        Database db;
        Map map;
        Radar radar;
        Options options;
        public InsolationService(Database db, Options options)
        {
            this.db = db;
            this.options = options;
            radar = new Radar(db, options);
            map = new Map(db, options);            
        }

        /// <summary>
        /// Расчет инсоляции в точке
        /// </summary>
        public void CalcPoint (Point3d pt)
        {
            var ms = db.CurrentSpaceId.GetObject( OpenMode.ForWrite) as BlockTableRecord;
            // Объекты в области действия точки
            var scope = map.GetScope(pt);
            // радар
            radar.Scan(pt, scope, ms);
        }
    }
}
