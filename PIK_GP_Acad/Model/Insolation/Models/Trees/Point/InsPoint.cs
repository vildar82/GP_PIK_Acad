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
        }

        [ExcludeFromSerialization]
        public IVisualInsPointIllums VisualIllums { get; private set; }

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
                VisualIllums = InsService.CreateVisualType<IVisualInsPointIllums>();
            }
            // Создание объектов визуализации
            VisualIllums.CreateVisual(this);            
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
