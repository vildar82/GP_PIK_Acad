using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib.Blocks;
using AcadLib.RTree.SpatialIndex;
using Autodesk.AutoCAD.Geometry;
using AcadLib;

namespace PIK_GP_Acad.Elements.Buildings
{
    /// <summary>
    /// Здание из блока
    /// </summary>
    public abstract class BuildingBlockBase : BuildingBase
    {
        private Rectangle r;

        public BuildingBlockBase(BlockReference blRef, string blName) : base(blRef.Id)
        {
            BlockBase = new BlockBase(blRef, blName);
            ExtentsInModel = blRef.GeometricExtentsСlean();
        }

        public BlockBase BlockBase { get; set; }
        public ObjectId IdPlContour { get; set; }

        public override Rectangle Rectangle {
            get {
                if (r == null)
                    r = GetRectangle();
                return r;
            }
            set { r = value; }
        }

        private Rectangle GetRectangle()
        {
            Extents3d ext;
            if (BlockBase.Bounds != null)
            {
                ext = BlockBase.Bounds.Value;
            }
            else
            {
                int halfBs = 20;
                ext = new Extents3d(new Point3d(BlockBase.Position.X - halfBs, BlockBase.Position.Y - halfBs, 0),
                    new Point3d(BlockBase.Position.X + halfBs, BlockBase.Position.Y + halfBs, 0));
            }
            Rectangle r = new Rectangle(ext);
            return r;
        }

        public override Polyline GetContourInModel()
        {
            if (!IdPlContour.IsValidEx()) return null;
            using (var pl = IdPlContour.Open(OpenMode.ForRead) as Polyline)
            {
                var plCopy = (Polyline)pl.Clone();
                plCopy.TransformBy(BlockBase.Transform);
                if (plCopy.Elevation != 0)
                    plCopy.Elevation = 0;
                return plCopy;
            }
        }
    }
}
