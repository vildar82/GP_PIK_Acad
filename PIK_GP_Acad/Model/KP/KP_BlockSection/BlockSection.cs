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
    class BlockSection
    {
        /// <summary>
        /// Полащадь секции по внешним границам стен
        /// </summary>
        public double AreaByExternalWalls { get; set; }
        /// <summary>
        /// Кол этажей
        /// </summary>
        public int Floors { get; set; } = 1;

        public BlockSection(BlockReference blRef)
        {
            // Определить параметры блок-секции: площадь,этажность
            Define(blRef);
        }

        private void Define(BlockReference blRef)
        {
            // Контурная полилиния - внешняя граница блок-секции по стенам.
            var plContour = PIK_GP_Acad.BlockSection.BlockSectionContours.FindContourPolyline(blRef);
            if(plContour == null)
            {
                Inspector.AddError("Не определен контур блок-секции",                    blRef, System.Drawing.SystemIcons.Error);
            }
            else
            {
                if(plContour.Area == 0)
                {
                    Inspector.AddError("Не определена площадь контура блок-секции", blRef, System.Drawing.SystemIcons.Error);
                }
                else
                {
                    AreaByExternalWalls = plContour.Area;
                }                
            }            

            // Определение этажности по атрибуту
            var attrs = AttributeInfo.GetAttrRefs(blRef);
            var atrFloor = attrs.Find(a => a.Tag.Equals(Options.Instance.BlockSectionAtrFloor));
            if(atrFloor == null)
            {
                Inspector.AddError("Не определен атрибут этажности блок-секции.",                     blRef, System.Drawing.SystemIcons.Error);                
            }
            else
            {
                int floor;
                if(int.TryParse(atrFloor.Text, out floor))
                {
                    Floors = floor;
                }
                else
                {
                    Inspector.AddError($"Не определена этажность по значению '{atrFloor.Text}' атрибута в блок-секции", blRef,
                        System.Drawing.SystemIcons.Error);
                }
            }
        }
    }
}
