using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Trees;
using AcadLib;
using Autodesk.AutoCAD.EditorInput;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчет елочек
    /// </summary>
    public class TreeModel
    {
        InsModel insModel;     

        public TreeModel(InsModel insModel)
        {
            this.insModel = insModel;
        }

        /// <summary>
        /// Расчетные точки
        /// </summary>
        public List<IInsPoint> Points { get; set; }

        /// <summary>
        /// Задание новой расчетной точки.
        /// Не добавляется в колекцию Points!
        /// Не расчитывается!
        /// Только выбор точки и ее параметров
        /// </summary>        
        public IInsPoint NewPoint ()
        {
            SelectPoint selPt = new SelectPoint();
            IInsPoint p = selPt.SelectNewPoint(insModel.Map);
            return p;            
        }
    }
}
