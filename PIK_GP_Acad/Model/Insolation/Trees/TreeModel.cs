using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Trees;
using AcadLib;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Catel.Data;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчет елочек
    /// </summary>
    public class TreeModel : ModelBase
    {
        InsModel insModel;     

        public TreeModel(InsModel insModel)
        {
            this.insModel = insModel;
            Points.CollectionChanged += Points_CollectionChanged;
        }

        /// <summary>
        /// Расчетные точки
        /// </summary>
        public ObservableCollection<IInsPoint> Points { get; set; }

        /// <summary>
        /// Задание новой расчетной точки.        
        /// </summary>        
        public void NewPoint ()
        {
            SelectPoint selPt = new SelectPoint();
            IInsPoint p = selPt.SelectNewPoint(insModel.Map);
            Points.Add(p);
        }
        
        /// <summary>
        /// Перенумерация точек при изменении порядка точек в коллекции 
        /// Точки нумеруются по порядку расположения в коллекции
        /// </summary>        
        private void Points_CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    var p = Points[i];
                    if (p.Number != i)
                        p.Number = i;
                }
            }
        }
    }
}
