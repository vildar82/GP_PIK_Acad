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
            VisualTrees = new VisualTree(InsModel);
            Points = new ObservableCollection<InsPoint>();
            //Points.CollectionChanged += Points_CollectionChanged;            
            IsVisualIllumsOn = true;
        }

        [ExcludeFromSerialization]
        public InsModel InsModel { get; set; }

        [ExcludeFromSerialization]
        public VisualTree VisualTrees { get; set; }
        
        /// <summary>
        /// Расчетные точки
        /// </summary>
        public ObservableCollection<InsPoint> Points { get; set; }
        public bool IsVisualIllumsOn { get; set; }
        public bool IsVisualTreeOn { get; set; }

        /// <summary>
        /// Обновление полное рачета елочек
        /// </summary>
        public void Update ()
        {
            foreach (var item in Points)
            {
                CalcPoint(item);
            }
            UpdateVisualTree(null);
        }

        /// <summary>
        /// Обновление визуализации елочек
        /// </summary>        
        public void UpdateVisualTree (InsPoint insPoint)
        {
            VisualTrees.Update();
        }

        /// <summary>
        /// Задание новой расчетной точки.        
        /// </summary>        
        public void AddPoint ()
        {
            SelectPoint selPt = new SelectPoint();
            InsPoint p = selPt.SelectNewPoint(InsModel);
            if (p != null)
            {
                CalcPoint(p);
                Points.Add(p);
                // Сразу включение зон инсоляции
                p.IsVisualIllumsOn = true;
            }
        }

        private void CalcPoint (InsPoint p)
        {
            try
            {
                p.Update();                
            }
            catch (Exception ex)
            {
                InsService.ShowMessage(ex, "Ошибка при добавлении точки.");
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
            //VisualTrees.Update(); // Обновляется в Points_CollectionChanged            
        }

        /// <summary>
        /// Точки принадлежащие зданию
        /// </summary>        
        public List<InsPoint> GetPointsInBuilding (InsBuilding building)
        {
            return Points.Where(p => p.Building == building).ToList();
        }

        /// <summary>
        /// Включение выключение всех визуализаций
        /// С сохранением состояния (вкл/выкл)
        /// </summary>        
        public void VisualsOnOff (bool onOff)
        {
            // Вкл/откл зон инсоляции точек с сохранением сосотояния
            VisualIllumsOnOff(onOff, true);
            VisualTreeOnOff(onOff, true);

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

        /// <summary>
        /// Включение/выключение зон инсоляции всех точек
        /// </summary>
        private void OnIsVisualIllumsOnChanged ()
        {
            VisualIllumsOnOff(IsVisualIllumsOn, false);
        }

        /// <summary>
        /// Включение/выключение визуализации зон инсоляции точек
        /// <param name="onOff">Null - по состоянию в точке, иначе принудительно</param>
        /// </summary>        
        private void VisualIllumsOnOff (bool onOff, bool saveState)
        {
            // Включение/выключение визуализации инсоляции всех точек
            if (Points != null)
            {
                foreach (var item in Points)
                {
                    // Изменение состояние на заданное                    
                    if (saveState)
                    {
                        item.VisualIllums.IsOn = onOff ? item.IsVisualIllumsOn : false;
                    }
                    else
                    {
                        item.IsVisualIllumsOn = onOff;
                    }
                }                
            }
        }

        /// <summary>
        /// Включение/отключение
        /// </summary>
        /// <param name="onOff">Вкл/выкл</param>
        /// <param name="saveState">Сохранение состояния</param>
        private void VisualTreeOnOff (bool onOff, bool saveState)
        {
            // Елочки
            if (saveState)
            {
                VisualTrees.IsOn = onOff ? IsVisualTreeOn : false;
            }
            else
            {
                VisualTrees.IsOn = onOff;
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
