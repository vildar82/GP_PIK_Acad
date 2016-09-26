using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Model.Insolation.ShadowMap;

namespace PIK_GP_Acad.Insolation
{
    public interface IMap
    {
        Document Doc { get; set; }
        List<Tile> Tiles { get; set; }
        InsBuilding GetBuildingInPoint (Point3d pt);
        Scope GetScope (Extents3d ext);
    }
}