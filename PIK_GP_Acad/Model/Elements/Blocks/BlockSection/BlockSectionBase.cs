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
using PIK_GP_Acad.BlockSection_GP;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Elements.InfraworksExport;
using PIK_GP_Acad.OD;
using PIK_GP_Acad.OD.Records;
using AcadLib.Errors;

namespace PIK_GP_Acad.Elements.Blocks.BlockSection
{
    /// <summary>
    /// Общий абстрактный класс для всех Блок-секций (основан гна здании из блока)
    /// </summary>
    public abstract class BlockSectionBase : BuildingBlockBase, IBuilding, IInfraworksExport
    {
        

        public const string PropHeightTypicalFloor = "H_Тип_этажа";
        public const string PropHeightFirstFloor = "H_1_этажа";
        public const string PropHeightTechFloor = "H_Тех_этажа";
        public const string PropElevation = "Уровень";

        /// <summary>
        /// Полащадь секции по внешним границам стен
        /// </summary>
        public double AreaGNS { get; set; }
        /// <summary>
        /// Полащадь секции - жилой площади
        /// </summary>
        public double AreaLive { get; set; }  

        public BlockSectionBase (BlockReference blRef, string blName) : base(blRef, blName)
        {
            //ExtentsInModel = BlockBase.Bounds.Value;
            // Определить параметры блок-секции: площадь,этажность  
            FriendlyTypeName = "Блок-секция";                                  
            Height = DefineHeight();
            // Относительный уровень
            Elevation = BlockBase.GetPropValue<double>(PropElevation, false, true);
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

        public List<IODRecord> GetODRecords ()
        {
            var odBuild = ODBuilding.GetRecord(BlockBase, IdPlContour, OD.Records.BuildingType.Live, Height);
            return new List<IODRecord> { odBuild };
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

        /// <summary>
        /// Определение высоты
        /// </summary>
        /// <returns></returns>
        private double DefineHeight()
        {   
            HeightFirstFloor = BlockBase.GetPropValue(PropHeightFirstFloor, 3.6, writeDefaultValue: true);
            HeightTypicalFloors = BlockBase.GetPropValue(PropHeightTypicalFloor, 2.9, writeDefaultValue: true);
            HeightTechnicalFloor = BlockBase.GetPropValue(PropHeightTechFloor, 1.6, writeDefaultValue: true);
            return CalcHeight(HeightFirstFloor, HeightTypicalFloors, HeightTechnicalFloor, Floors);
        }

        /// <summary>
        /// Определение высоты здания по высотам 1, типового и тех этажей
        /// </summary>
        /// <param name="h1">Высота 1 этажа, м</param>
        /// <param name="hTypical">Высота типового этажа, м</param>
        /// <param name="hTech">Высота тех этажа, м</param>
        /// <param name="floors">Кол. этажей</param>
        /// <returns>Высота здания, м</returns>
        public static double CalcHeight(double h1, double hTypical, double hTech, int floors)
        {
            if (h1 == 0) h1 = 3.6;
            if (hTypical == 0) hTypical = 2.9;
            if (hTech == 0) hTech = 1.6;
            if (floors == 0) floors = 1;
            return h1 + (floors - 1) * hTypical + hTech;
        }
    }
}
