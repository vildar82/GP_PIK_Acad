using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Blocks;
using AcadLib.RTree.SpatialIndex;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
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
        public string HouseName { get; set; }

        public string PluginName { get; set; } = "GP";

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
            // Загрузка значений из словаря объекта
            this.LoadDboDict();
        }

        public Polyline GetContourInModel ()
        {
            using (var pl = IdPlContour.Open(OpenMode.ForRead) as Polyline)
            {
                var plCopy = (Polyline)pl.Clone();
                plCopy.TransformBy(Transform);
                if (plCopy.Elevation != 0)
                    plCopy.Elevation = 0;
                return plCopy;
            }
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

        /// <summary>
        /// Корректировка контура блок-секции пересекающейся с другой блок секцией
        /// Случай, когда блок-секции соединяются по одной из граней контура - По Оси стены (внешние контуры здания при этом пересекаются, что недопустимо)
        /// </summary>
        /// <param name="pl">Контур корректируемой б.с.</param>
        /// <param name="plOther">Контур другой б.с. (границы которой попадают в границы перврй б.с.)</param>
        public static void CorrectSectionsConnect (ref Polyline pl, Polyline plOther)
        {
            // Точки пересечения блок секций
            Point3dCollection ptIntersects = new Point3dCollection();
            pl.IntersectWith(plOther, Intersect.OnBothOperands, new Plane(), ptIntersects, IntPtr.Zero, IntPtr.Zero);

            if (ptIntersects.Count == 0) return;

            int numVertex = pl.NumberOfVertices;
            var modifiedPoints = new List<Point2d>();

            // для каждой точки пересечения найти ближайшую вершину на двух полилиниях
            foreach (Point3d ptIntersect in ptIntersects)
            {
                var ptClosest = pl.GetClosestPointTo(ptIntersect, true);
                var param = pl.GetParameterAtPoint(ptClosest);
                var paramIndex = Convert.ToInt32(param);
                var ptVertexNearest = pl.GetPointAtParameter(paramIndex).Convert2d();
                int vertexIndex = paramIndex == numVertex ? 0 : paramIndex;

                if (modifiedPoints.Contains(ptVertexNearest))
                {
                    continue;
                }

                var ptClosestItem = plOther.GetClosestPointTo(ptIntersect, true);
                var paramItem = plOther.GetParameterAtPoint(ptClosestItem);
                var paramIndexItem = Convert.ToInt32(paramItem);
                var ptVertexNearestItem = plOther.GetPointAtParameter(paramIndexItem).Convert2d();

                if ((ptVertexNearest - ptVertexNearestItem).Length > 5)
                {
                    continue;
                }

                Point2d ptInsert = ptVertexNearest.Center(ptVertexNearestItem);

                pl.RemoveVertexAt(vertexIndex);
                pl.AddVertexAt(vertexIndex, ptInsert, 0, 0, 0);

                modifiedPoints.Add(ptInsert);
            }
        }

        public DBObject GetDBObject ()
        {
            return IdEnt.GetObject(OpenMode.ForWrite);
        }

        public DicED GetExtDic (Document doc)
        {
            var dicBuild = new DicED("Building");
            dicBuild.AddRec("Values", GetDataValues(doc));
            return dicBuild;
        }

        public void SetExtDic (DicED dicEd, Document doc)
        {
            if (dicEd == null) return;
            var dicBuild = dicEd.GetInner("Building");
            SetDataValues(dicBuild.GetRec("Values")?.Values, doc);
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            return new List<TypedValue> {
                TypedValueExt.GetTvExtData(HouseName)
            };
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            if (values == null || values.Count !=1)
            {
                // Дефолт
            }
            else
            {
                HouseName = TypedValueExt.GetTvValue<string>(values[0]);
            }
        }
    }
}
