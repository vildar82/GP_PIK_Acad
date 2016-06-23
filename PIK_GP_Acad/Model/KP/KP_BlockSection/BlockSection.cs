using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.KP.KP_BlockSection
{
    /// <summary>
    /// блок-секции концепции - пока общий класс для Рядовой и Угловой
    /// </summary>
    public class BlockSection : BlockBase
    {
        /// <summary>
        /// Полащадь секции по внешним границам стен
        /// </summary>
        public double AreaByExternalWalls { get; set; }
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

        private void Define(BlockReference blRef)
        {
            // Контурная полилиния - внешняя граница блок-секции по стенам.
            Polyline plAxis;
            var plContour = PIK_GP_Acad.BlockSection.BlockSectionContours.FindContourPolyline(blRef, out plAxis,KP_BlockSectionService.blKpParkingLayerContour);
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
                    AreaByExternalWalls = plContour.Area;
                }                
            } 
            
            if (plAxis == null || plAxis.Area == 0)
            {
                throw new Exception($"Не определена площадь жилой площади блок-секции - по полилинии на слое {KP_BlockSectionService.blKpParkingLayerContour}");
            }
            else
            {
                AreaLive = plAxis.Area;
            }

            // Определение этажности по атрибуту
            Floors = GetPropValue<int>(Options.Instance.BlockSectionAtrFloor);
            //var attrs = AttributeInfo.GetAttrRefs(blRef);            
            //var atrFloor = attrs.Find(a => a.Tag.Equals(Options.Instance.BlockSectionAtrFloor));
            //if(atrFloor == null)
            //{
            //    Inspector.AddError("Не определен атрибут этажности блок-секции.", blRef, System.Drawing.SystemIcons.Error);                
            //}
            //else
            //{
            //    int floor;
            //    if(int.TryParse(atrFloor.Text, out floor))
            //    {
            //        Floors = floor;
            //    }
            //    else
            //    {
            //        Inspector.AddError($"Не определена этажность по значению '{atrFloor.Text}' атрибута в блок-секции", blRef,
            //            System.Drawing.SystemIcons.Error);
            //    }
            //}
        }
    }
}
