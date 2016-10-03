using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using Catel.Data;
using Catel.Runtime.Serialization;
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

        public InsPoint () { }

        public InsPoint (InsModel model)
        {
            this.model = model;
            Window = new WindowOptions();
            Number = model.Tree.Points.Count+1;           
        }

        [ExcludeFromSerialization]
        public VisualInsPointIllums VisualIllums { get; private set; }
        [ExcludeFromSerialization]
        public  VisualInsPointInfo VisualPointInfo { get; private set; }

        /// <summary>
        /// Номер точки
        /// </summary>
        public int Number { get; set; }
        public Point3d Point { get; set;}
        [ExcludeFromSerialization]
        public InsBuilding Building { get; set; }
        [ExcludeFromSerialization]
        public List<IIlluminationArea> Illums { get; set; }
        [ExcludeFromSerialization]
        public InsValue InsValue { get; set; }        
        public int Height { get; set; }
        public WindowOptions Window { get; set; }
        public bool IsVisualIllumsOn { get; set; }

        /// <summary>
        /// Расчет точки - зон освещенности и времени
        /// </summary>
        public void Calc ()
        {
            Illums = model.CalcService.TreesCalc.CalcPoint(this, model.Map);
            InsValue = model.CalcService.CalcTimeAndGetRate(Illums);

            // Визуализация зон инсоляции точки
            if (VisualIllums == null)
            {
                VisualIllums = new VisualInsPointIllums();
            }            
            VisualIllums.CreateVisual(this);

            // Визуализация описания точки
            if (VisualPointInfo == null)
                VisualPointInfo = new VisualInsPointInfo(this);
            else
                VisualPointInfo.InsPoint = this;
            if (model.IsInsActivated)
                VisualPointInfo.IsOn = true;
        }        
        
        private void OnIsVisualIllumsOnChanged ()
        {
            // Включение/выключение визуализации инсоляционных зон точки
            if (VisualIllums != null && model.Tree.IsVisualIllumsOn)
            {
                VisualIllums.IsOn = IsVisualIllumsOn;
            }
        }
    }
}
