using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using AcadLib;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Catel.Data;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчет елочек
    /// </summary>
    public class TreeModel : ModelBase
    {
        /// <summary>
        /// Для загрузки расчета
        /// </summary>
        public TreeModel () { }
        /// <summary>
        /// Создание нового расчета
        /// </summary>
        /// <param name="insModel">Модель инсоляции</param>
        public TreeModel(InsModel insModel)
        {
            this.InsModel = insModel;
            Points = new ObservableCollection<InsPoint>();
            Points.CollectionChanged += Points_CollectionChanged;
            VisualOptions = InsService.Settings.TreeVisualOptions;
        }
        public InsModel InsModel { get; set; }

        public ObservableCollection<TreeVisualOption> VisualOptions { get; set; } 
        /// <summary>
        /// Расчетные точки
        /// </summary>
        public ObservableCollection<InsPoint> Points { get; set; }
        /// <summary>
        /// Задание новой расчетной точки.        
        /// </summary>        
        public void NewPoint ()
        {
            SelectPoint selPt = new SelectPoint();
            InsPoint p = selPt.SelectNewPoint(InsModel);
            if (p != null)
            {
                // TODO: расчет точки    
                p.Calc();                            
                Points.Add(p);
            }
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
