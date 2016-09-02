using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Elements;
using PIK_GP_Acad.Insolation.Central;
using PIK_GP_Acad.Model.Insolation.ShadowMap.Visualization;

namespace PIK_GP_Acad.Insolation.Central
{
    /// <summary>
    /// Расчет инсоляции в центральном регионе - лучевой конус это плоскость, в день равноденствия
    /// </summary>
    public class CentralInsService : IInsolationService
    {
        public Database Db { get; set; }
        public Map Map { get; set; } 
        public CalcValuesCentral CalcValues { get; set; }
        public InsOptions Options { get; set; }

        public CentralInsService(Database db, InsOptions options)
        {
            Db = db;
            Options = options;
            CalcValues = new CalcValuesCentral(options);
            Map = new Map(db, options);            
        }       

        /// <summary>
        /// Расчет инсоляции в точке
        /// </summary>
        public void CalcPoint (Point3d pt)
        {
            using (var t = Db.TransactionManager.StartTransaction())
            {
                var ms = Db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                var calcPt = new CalcPointCentral(pt, this);                
                // Расчет освещенности в точке
                var illumAreas = calcPt.Calc();
                // Построение зон освещенности
                cretateIllumAreas(illumAreas);

                t.Commit();
            }
        }

        public void CreateShadowMap()
        {
            Visual visual = new Visual();
            visual.Show(Map);
        }             

        private void cretateIllumAreas (List<IlluminationArea> res)
        {
            var cs = Db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
            foreach (var illum in res)
            {
                illum.Create(cs);
            }
        }
    }
}
