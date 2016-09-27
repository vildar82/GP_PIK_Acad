﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using Catel.Data;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчетная точка
    /// </summary>
    public class InsPoint : ModelBase
    {
        InsModel model;
        public InsPoint (InsModel model)
        {
            this.model = model;
            Window = new WindowOptions();  
        }

        /// <summary>
        /// Номер точки
        /// </summary>
        public int Number { get; set; }
        public Point3d Point { get; set;}
        public InsBuilding Building { get; set; }
        public List<IIlluminationArea> Illums { get; set; }
        public InsRequirement InsReq { get; set; }
        public BuildingTypeEnum BuildingType { get; set; }       
        public int Height { get; set; }
        public WindowOptions Window { get; set; }

        /// <summary>
        /// Расчет точки - зон освещенности и времени
        /// </summary>
        public void Calc ()
        {
            Illums = model.CalcService.TreesCalc.CalcPoint(this, model.Map);
            InsReq = model.CalcService.CalcTimeAndGetRate(Illums);
        }
    }
}
