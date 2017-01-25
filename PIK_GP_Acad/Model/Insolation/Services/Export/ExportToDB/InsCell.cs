﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Models;
using AcadLib;
using AcadLib.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Services.Export
{
    /// <summary>
    /// Ячейка инсоляции
    /// </summary>
    public class InsCell
    {
        public const double ModuleSize = 3.6;
        public static readonly Tolerance Tolerance = new Tolerance(0.01,1.5);
        
        public static int IndexCounter { get; set;}

        public InsCell(Point2d cellPt, List<FrontCalcPoint> calcPts, Vector2d vecModule)
        {
            this.Direction = vecModule;
            PtCenter = cellPt;
            PtCenterOrig = cellPt;
            // Определение значения инсоляции
            InsValue = GetInsValue(calcPts);
            Index = IndexCounter++;
        }

        public Vector2d Direction { get; set; }
        /// <summary>
        /// Индекс строки (ряда) от 0
        /// </summary>
        public int Row { get; set; }
        /// <summary>
        /// Индекс столбца с 0
        /// </summary>
        public int Column { get; set; }
        public InsRequirementEnum InsValue { get; set; }

        public Point2d PtCenter { get; set; }
        public Point2d PtCenterOrig { get; set; }
        public int Index { get; set; }             

        /// <summary>
        /// Определение значения инсоляции ячейки по набору расчетных точек в этом модуле
        /// </summary>
        /// <param name="calcPts">Расчетные точки в этом модуле (ячейке)</param>
        /// <returns>Значение инсоляции модуля</returns>
        private InsRequirementEnum GetInsValue(List<FrontCalcPoint> calcPts)
        {
            var insVal = calcPts.GroupBy(g => g.InsValue).OrderByDescending(o => o.Count()).First().Key;
            //var insVal = (InsRequirementEnum)(calcPts.Average(s => (int)s.InsValue));
            if (insVal == InsRequirementEnum.None)
            {
                insVal = InsRequirementEnum.A;
            }
            // Если значение чуток не дотягивает до B (=A1), то приравнять ее к B (генпланисты сделают подгонку)
            else if (insVal == InsRequirementEnum.A1)
            {
                insVal = InsRequirementEnum.B;
            }
            return insVal;
        }

        public string GetInsValue()
        {
            return InsValue.ToString();
        }

        /// <summary>
        /// Только для угловых ячеек - наложение модулей от разных сегментов
        /// </summary>
        /// <param name="cell"></param>
        public bool Overlay(InsCell cell)
        {
            // Модули должны быть перпендикулярно направлены
            if (!Direction.IsPerpendicularTo(cell.Direction, Tolerance))
            {                
                return false;
            }
            // Точки центров модулей должны совпадать
            if (!PtCenter.IsEqualTo(cell.PtCenter, Tolerance))
            {
                return false;
            }                
            
            // Значение инсоляции - лучшее из двух
            if (InsValue< cell.InsValue)
            {
                InsValue = cell.InsValue;
            }
            return true;
        }

        public override string ToString()
        {
            return $"Ячейка: центр={PtCenter}, направление={Direction}, значение={InsValue}, [{Row};{Column}]";
        }

        /// <summary>
        /// Определение номера строки и столбца ячейки
        /// </summary>
        public void DefineNumCell()
        {
            Row = (int)Math.Floor(-PtCenter.Y / InsCell.ModuleSize) +1; // - т.к. в автокаде ось Y направлена вверх, а отсчет рядов идет вниз (группа перемещена в 4 четверть)
            Column = (int)Math.Floor(PtCenter.X / InsCell.ModuleSize) +1;
        }

        /// <summary>
        /// Тестовая отрисовка ячейки
        /// </summary>
        public void TestDraw()
        {
            var ext = PtCenter.GetRectangleFromCenter(InsCell.ModuleSize);
            var pl = ext.GetPolyline();
            EntityHelper.AddEntityToCurrentSpace(pl);
            EntityHelper.AddEntityToCurrentSpace(new DBPoint(PtCenter.Convert3d()));
            var text = new DBText();
            text.TextString = $"[{Row};{Column}]";
            text.Height = 1;
            text.Justify = AttachmentPoint.MiddleCenter;
            text.AlignmentPoint = PtCenter.Convert3d();
            text.AdjustAlignment(HostApplicationServices.WorkingDatabase);
            EntityHelper.AddEntityToCurrentSpace(text);

            var textIndex = new DBText();
            textIndex.TextString = Index.ToString();
            textIndex.Height = 0.25;
            textIndex.Justify = AttachmentPoint.MiddleCenter;
            textIndex.AlignmentPoint = new Point3d(PtCenter.X, PtCenter.Y-1.5, 0);
            textIndex.AdjustAlignment(HostApplicationServices.WorkingDatabase);
            EntityHelper.AddEntityToCurrentSpace(textIndex);
        }
    }
}
