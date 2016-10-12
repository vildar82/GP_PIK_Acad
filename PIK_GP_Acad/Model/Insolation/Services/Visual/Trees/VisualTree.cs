using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class VisualTree : VisualServiceBase
    {
        public ObservableCollection<InsPoint> Points { get; set; }
                
        public InsModel Model { get; set; }
        public VisualTree(InsModel model)
        {            
            Model = model;            
        }

        public override List<Drawable> CreateVisual ()
        {
            List<Drawable> drawsAllPointsTrees = new List<Drawable>();
            foreach (var item in Points)
            {
                var drawsPointTrees = GetTreeDraws(item);
                drawsAllPointsTrees.AddRange(drawsPointTrees);
            }
            return drawsAllPointsTrees;
        }

        private List<Drawable> GetTreeDraws (InsPoint insPoint)
        {
            List<Drawable> drawsInsPointTrees = new List<Drawable>();

            Point2d p1 = insPoint.Point.Convert2d();
            Point2d p2= p1;
            Point2d p3;
            Point2d p4;

            var treeVisOptions = insPoint.Model.Tree.TreeOptions.TreeVisualOptions;
            foreach (var treeVisOpt in treeVisOptions)
            {
                var draws = GetDrawsByOption(insPoint, treeVisOpt, p1, p2, out p3, out p4);
                drawsInsPointTrees.AddRange(draws);
            }
            return drawsInsPointTrees;
        }

        /// <summary>
        /// Графика визуализации заданной высотности (настройка визуализации елочек) 
        /// </summary>
        /// <param name="insPoint">инс точка</param>
        /// <param name="treeOpt">Настройка визуализации высоты улочек</param>
        /// <param name="p1">Первая точка елочки (левый верхний угол прямоугольника). Для первой елочки p1=p2=insPoint</param>
        /// <param name="p2">Правый верхний угол елочки</param>
        /// <param name="p3">Нижний правый угол елочки (возвращается)</param>
        /// <param name="p4">Нижений левый угол елочки (возвращается)</param>
        /// <returns></returns>
        private List<Drawable> GetDrawsByOption (InsPoint insPoint, TreeVisualOption treeOpt, 
            Point2d p1, Point2d p2, out Point2d p3, out Point2d p4)
        {
            List<Drawable> draws = new List<Drawable>();

            var ptOrig = insPoint.Point.Convert2d();
            var calcValues = Model.CalcService.CalcValues;

            double cShadow;
            // Высота тени (на заданной настройкой высоте елочки) - катет по Y
            var yShadow = calcValues.YShadowLineByHeight(treeOpt.Height, out cShadow);

            // Луч падения - катет по X до точки на луче, на заданой высоте
            var xRay = calcValues.GetXRay(yShadow, insPoint.AngleStartOnPlane);
            p3 = ptOrig + new Vector2d(xRay, -yShadow);

            xRay = calcValues.GetXRay(yShadow, insPoint.AngleEndOnPlane);
            p4 = ptOrig + new Vector2d(xRay, -yShadow);

            var visOpt = new VisualOption(treeOpt.Color, Point3d.Origin, 60);
            var points = new List<Point2d> { p1, p2, p3, p4 };

            var h = CreateHatch(points, visOpt);
            draws.Add(h);
            return draws;
        }
    }
}
