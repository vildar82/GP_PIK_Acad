using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using AcadLib.RTree.SpatialIndex;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.BlockSection;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Elements.InfraworksExport;
using PIK_GP_Acad.OD;
using PIK_GP_Acad.OD.Records;

namespace PIK_GP_Acad.Elements.Blocks.BlockSection
{
    public abstract class BlockSectionBase : BlockBase, IBuilding, IInfraworksExport
    {
        private Rectangle r;
        public ObjectId IdEnt { get; private set; }
        public int Floors { get; set; } = 1;
        public int Height { get; set; }
        /// <summary>
        /// Полащадь секции по внешним границам стен
        /// </summary>
        public double AreaGNS { get; set; }
        /// <summary>
        /// Полащадь секции - жилой площади
        /// </summary>
        public double AreaLive { get; set; }        
        public ObjectId IdPlContour { get; set; }
        public Extents3d ExtentsInModel { get; set; }
        
        public Rectangle Rectangle {
            get {
                if (r == null)
                    r = GetRectangle();
                return r;
            }
        }

        public BuildingTypeEnum BuildingType { get; set; } = BuildingTypeEnum.Living;

        public BlockSectionBase (BlockReference blRef, string blName) : base(blRef, blName)
        {
            IdEnt = blRef.Id;
            ExtentsInModel = this.Bounds.Value;
            // Площадь по внешней полилинии
            Polyline plLayer;
            var plContour = BlockSectionContours.FindContourPolyline(blRef, out plLayer);
            if (plContour == null)
            {
                throw new Exception("Не определен контур блок-секции");
            }
            else
            {
                IdPlContour = plContour.Id;
                AreaGNS = plContour.Area;
            }
        }

        public Polyline GetContourInModel ()
        {
            var pl = IdPlContour.GetObject(OpenMode.ForRead) as Polyline;
            var plCopy = (Polyline)pl.Clone();
            plCopy.TransformBy(Transform);
            return plCopy;
        }

        protected virtual void Define (BlockReference blRef)
        {
            Height = Floors * 3 + 3;
        }

        public List<IODRecord> GetODRecords ()
        {
            var odBuild = ODBuilding.GetRecord(this, IdPlContour, OD.Records.BuildingType.Live, Height);
            return new List<IODRecord> { odBuild };
        }

        private Rectangle GetRectangle ()
        {
            Extents3d ext;
            if (Bounds != null)
            {
                ext = Bounds.Value;
            }
            else
            {
                int halfBs = 20;
                ext = new Extents3d(new Point3d(Position.X - halfBs, Position.Y - halfBs, 0),
                    new Point3d(Position.X + halfBs, Position.Y + halfBs, 0));
            }
            Rectangle r = new Rectangle(ext);
            return r;
        }
    }
}
