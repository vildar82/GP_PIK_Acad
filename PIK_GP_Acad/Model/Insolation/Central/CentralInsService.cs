using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Elements;
using PIK_GP_Acad.Insolation.Central;
using PIK_GP_Acad.Insolation.Options;
using PIK_GP_Acad.Model.Insolation.ShadowMap.Visualization;

namespace PIK_GP_Acad.Insolation.Central
{
    /// <summary>
    /// Расчет инсоляции в центральном регионе - лучевой конус это плоскость, в день равноденствия
    /// </summary>
    public class CentralInsService : IInsolationService
    {
        Document doc;
        public Database Db { get; set; }
        public Map Map { get; set; } 
        public CalcValuesCentral CalcValues { get; set; }
        public InsOptions Options { get; set; }
        /// <summary>
        /// Расчет Елочек
        /// </summary>
        public IInsTreeService Trees { get; set; }

        public CentralInsService(Document doc, Options.InsRegion region)
        {
            this.doc = doc;
            Options = new InsOptions();
            Options.Region = region;
            Trees = new TreesCentral(this);
            Db = doc.Database;                        
            // загрузка карты (зданий с чертежа)
            Map = new Map(doc);
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

        /// <summary>
        /// Карта теней
        /// </summary>
        public void CreateShadowMap()
        {
            Visual visual = new Visual();
            visual.Show(Map);
        }             

        private void cretateIllumAreas (List<IIlluminationArea> res)
        {
            var cs = Db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
            foreach (var illum in res)
            {
                illum.Create(cs);
            }
        }
    }
}
