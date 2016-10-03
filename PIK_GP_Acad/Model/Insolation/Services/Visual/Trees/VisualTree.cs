using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using PIK_GP_Acad.Insolation.Models;
using static PIK_GP_Acad.Insolation.Services.VisualHelper;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Визуализация клочек
    /// </summary>
    public class VisualTree : VisualServiceBase, IVisual
    {        
        List<KeyValuePair<InsPoint, List<Drawable>>> points = new List<KeyValuePair<InsPoint, List<Drawable>>>();

        public List<TreeVisualOption> VisualOptions { get; set; }
        public InsModel Model { get; set; }
        public VisualTree(InsModel model,List<TreeVisualOption> visualOptions)
        {
            VisualOptions = visualOptions;
            Model = model;
        }

        public void AddPoint (InsPoint p)
        {
            //var draws = GetDraws(p);
            //points.Add(new KeyValuePair<InsPoint, List<Drawable>>(p, draws));
        }        

        public List<Drawable> CreateVisual ()
        {
            return points.SelectMany(s => s.Value).ToList();
        }

        //private List<Drawable> GetDraws (InsPoint p)
        //{            
        //    var opt = VisualOptions[0];            
        //    var draws = GetDrawsByOption(opt);
        //}

        //private List<Drawable> GetDrawsByOption (InsPoint insPoint, TreeVisualOption opt, Point3d p1, Point3d p2, out Point3d p3, out Point3d p4)
        //{            
        //    double cShadow;
        //    var yShadow = Model.CalcService.CalcValues.YShadowLineByHeight(opt.Height, out cShadow);

        //    //p3 = IllumAreaBase.GetPointInRayByHeight(yShadow, insPoint.);

        //    var visOpt = new VisualOption(opt.Color, Point3d.Origin, 60);
        //    //var h = CreateHatch()
        //}
    }
}
