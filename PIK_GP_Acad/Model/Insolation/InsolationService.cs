using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Elements;
using PIK_GP_Acad.Insolation.SunlightRule;
using PIK_GP_Acad.Model.Insolation.ShadowMap.Visualization;

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
        InsOptions options;
        public InsolationService(Database db, InsOptions options)
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
            using (var t = db.TransactionManager.StartTransaction())
            {
                var ms = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                // Объекты в области действия точки
                var scope = map.GetScopeInPoint(pt);
                // радар
                var res = radar.Scan(pt, scope, ms);
                // Построение зон освещенности
                cretateIllumAreas(res);

                t.Commit();
            }
        }

        public void CreateShadowMap()
        {
            Visual visual = new Visual();
            visual.Show(map);
        }             

        private void cretateIllumAreas (List<IlluminationArea> res)
        {
            var cs = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
            foreach (var illum in res)
            {
                illum.Create(cs);
            }
        }
    }
}
