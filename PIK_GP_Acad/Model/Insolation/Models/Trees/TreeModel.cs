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
using System.ComponentModel;
using Catel.Runtime.Serialization;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчет елочек
    /// </summary>
    public class TreeModel : ModelBase
    {
        private Tolerance tolerancePoints = new Tolerance(1, 1);

        /// <summary>
        /// Для загрузки расчета
        /// </summary>
        public TreeModel ()
        {           

        }
        /// <summary>
        /// Создание нового расчета
        /// </summary>
        /// <param name="insModel">Модель инсоляции</param>
        public TreeModel(InsModel insModel)
        {
            this.InsModel = insModel;
            VisualOptions = InsService.Settings.TreeVisualOptions;
            VisualTrees = new VisualTree(InsModel, VisualOptions.ToList());
            Points = new ObservableCollection<InsPoint>();
            //Points.CollectionChanged += Points_CollectionChanged;            
            IsVisualIllumsOn = true;
        }
        [ExcludeFromSerialization]
        public InsModel InsModel { get; set; }
        [ExcludeFromSerialization]
        public VisualTree VisualTrees { get; set; }
        public ObservableCollection<TreeVisualOption> VisualOptions { get; set; } 
        /// <summary>
        /// Расчетные точки
        /// </summary>
        public ObservableCollection<InsPoint> Points { get; set; }
        public bool IsVisualIllumsOn { get; set; }
        public bool IsVisualTreeOn { get; set; }
        /// <summary>
        /// Задание новой расчетной точки.        
        /// </summary>        
        public void AddPoint ()
        {
            SelectPoint selPt = new SelectPoint();
            InsPoint p = selPt.SelectNewPoint(InsModel);
            if (p != null)
            {
                p.Calc();
                Points.Add(p);

                p.IsVisualIllumsOn = true;

                //VisualTrees.AddPoint(p);                
            }
        }

        /// <summary>
        /// Показать точку на чертеже
        /// </summary>        
        public void ShowPoint (InsPoint selectedPoint)
        {
            if (selectedPoint == null) return;

            var doc = InsModel.Doc;
            using (doc.LockDocument())
            {
                Editor ed = doc.Editor;
                var point = selectedPoint.Point;
                double delta = 5;
                Extents3d extPoint = new Extents3d(new Point3d(point.X - delta, point.Y - delta, 0),
                                                   new Point3d(point.X + delta, point.Y + delta, 0));
                ed.Zoom(extPoint);
            }
        }

        /// <summary>
        /// Удаление точки
        /// </summary>        
        public void DeletePoint (InsPoint insPoint)
        {
            Points.Remove(insPoint);
            VisualTrees.Update();
        }

        /// <summary>
        /// Включение выключение всех визуализаций
        /// Выключение - принудительное
        /// Включение - по состоянию
        /// </summary>        
        public void VisualsOnOff (bool onOff)
        {
            if (onOff)
            {
                // Включение - по состоянию
                VisualIllumsOnOff(IsVisualIllumsOn);
                VisualTrees.IsOn = IsVisualTreeOn;
            }
            else
            {
                // Выключение - принудительное
                VisualIllumsOnOff(onOff);
                VisualTrees.IsOn = onOff;
            }

            // Включение/отключение описания точек (подпись точек)
            foreach (var item in Points)
            {
                item.VisualPointInfo.IsOn = onOff;
            }
        }

        /// <summary>
        /// Перенумерация точек при изменении порядка точек в коллекции 
        /// Точки нумеруются по порядку расположения в коллекции
        /// </summary>        
        private void OnPointsChanged ()
        {
            Points.CollectionChanged += Points_CollectionChanged;
            VisualTrees.Points = Points;            
        }
        private void OnIsVisualIllumsOnChanged ()
        {
            VisualIllumsOnOff(IsVisualIllumsOn);
        }

        /// <summary>
        /// Включение/выключение визуализации зон инсоляции точек
        /// </summary>        
        public void VisualIllumsOnOff (bool onOff)
        {
            // Включение/выключение визуализации инсоляции всех точек
            if (Points != null)
            {
                foreach (var item in Points)
                {
                    item.IsVisualIllumsOn = onOff;
                }
            }
        }

        private void OnIsVisualTreeOnChanged()
        {
            VisualTrees.IsOn = IsVisualTreeOn;
        }

        /// <summary>
        /// Проверка есть ли уже такая точка в списке точек инсоляции
        /// </summary>        
        public bool HasPoint (Point3d pt)
        {
            var res = Points.Any(p => p.Point.IsEqualTo(pt, tolerancePoints));
            return res;
        }

        private void Points_CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                var p = Points[i];
                var num = i + 1;
                if (p.Number != num)
                {
                    p.Number = num;
                    p.VisualPointInfo.Update();
                }
            }
            VisualTrees.Update();
        }
    }
}
