using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    public interface IIlluminationArea
    {
        InsPoint InsPoint { get; set; }
        Point2d PtOrig { get; set; }
        Point2d PtStart { get; set; }
        Point2d PtEnd { get; set; }
        double AngleEndOnPlane { get; set; }
        double AngleStartOnPlane { get; set; }
        int Time { get; set; }
        List<Entity> CreateVisual ();
        Vector2d GetMidVector ();
        /// <summary>
        /// Смена стартового и конечного углов
        /// </summary>
        void Invert ();
    }
}