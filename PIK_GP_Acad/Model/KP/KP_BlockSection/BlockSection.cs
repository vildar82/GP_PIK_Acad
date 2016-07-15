using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using AcadLib.Errors;
using AcadLib.RTree.SpatialIndex;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.KP.KP_BlockSection
{
    /// <summary>
    /// блок-секции концепции - пока общий класс для Рядовой и Угловой
    /// </summary>
    public class BlockSection : BlockBase
    {
        private Rectangle r;
        public Rectangle Rectangle {
            get {
                if (r == null)
                    r = GetRectangle();
                return r;
            }
        }
        /// <summary>
        /// Полилиния по осям - в блоке БС
        /// </summary>
        public ObjectId PlAxisId { get; set; }
        /// <summary>
        /// Полилиния по ГНС (границы наружных стен) - в блоке БС
        /// </summary>
        public ObjectId PlExternalId { get; set; }

        /// <summary>
        /// Полащадь секции по внешним границам стен
        /// </summary>
        public double AreaGNS { get; set; }
        /// <summary>
        /// Полащадь секции - жилой площади
        /// </summary>
        public double AreaLive { get; set; }
        /// <summary>
        /// Кол этажей
        /// </summary>
        public int Floors { get; set; } = 1;        

        public BlockSection(BlockReference blRef, string blName) : base (blRef, blName)
        {
            // Определить параметры блок-секции: площадь,этажность            
            Define(blRef);            
        }

        protected virtual void Define(BlockReference blRef)
        {
            // Контурная полилиния - внешняя граница блок-секции по стенам.
            Polyline plAxis;
            var plContour = PIK_GP_Acad.BlockSection.BlockSectionContours.FindContourPolyline(blRef, out plAxis);// KP_BlockSectionService.blKpParkingLayerAxisContour);
            if(plContour == null)
            {
                throw new Exception("Не определен контур блок-секции");
            }
            else
            {
                if(plContour.Area == 0)
                {
                    throw new Exception("Не определена площадь контура блок-секции");
                }
                else
                {
                    AreaGNS = plContour.Area;
                }                
            } 
            
            //if (plAxis == null || plAxis.Area == 0)
            //{
            //    throw new Exception($"Не определена жилая площадь (в осях) блок-секции по полилинии на слое {KP_BlockSectionService.blKpParkingLayerAxisContour}");
            //}
            //else
            //{
            //    AreaLive = plAxis.Area;
            //}

            //PlAxisId = plAxis.Id;
            PlExternalId = plContour.Id;

            // Определение этажности по атрибуту
            Floors = GetPropValue<int>(Options.Instance.BlockSectionAtrFloor, exactMatch: false);                                   
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
            Rectangle r = new Rectangle (ext);
            return r;
        }
    }
}
