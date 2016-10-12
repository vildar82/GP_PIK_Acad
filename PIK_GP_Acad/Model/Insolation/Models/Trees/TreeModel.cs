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
    [Serializable]
    public class TreeModel : ModelBase
    {
        private static Tolerance tolerancePoints = new Tolerance(1, 1);

        [ExcludeFromSerialization]
        public InsModel Model { get; set; }

        [ExcludeFromSerialization]
        private VisualTree VisualTrees { get; set; }

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
            this.Model = insModel;            
            VisualTrees = new VisualTree(Model);
            Points = new ObservableCollection<InsPoint>();
        }                

        /// <summary>
        /// Расчетные точки
        /// </summary>
        [ExcludeFromSerialization]
        public ObservableCollection<InsPoint> Points { get; private set; }

        /// <summary>
        /// Включение/выключение зон инсоляции для всех точек
        /// </summary>
        public bool IsVisualIllumsOn { get; set; }
        public bool IsVisualTreeOn { get; set; }

        /// <summary>
        /// Обновление полное рачета елочек
        /// </summary>
        public void Update ()
        {
            foreach (var item in Points)
            {
                item.DefineBuilding();
                item.Update();
            }
            UpdateVisualTree(null);
        }        

        /// <summary>
        /// Обновление визуализации елочек
        /// </summary>        
        public void UpdateVisualTree (InsPoint insPoint)
        {
            VisualTrees.VisualUpdate();
        }

        /// <summary>
        /// Расчет и добавление точки
        /// </summary>        
        public void AddPoint (InsPoint p)
        {            
            // Расчет и добавление точки
            if (p != null)
            {
                // определение здания, если еще не определено
                if (p.Building == null)
                {
                    p.DefineBuilding();
                }
                p.CreatePoint();
                Points.Add(p);                
                p.Update();
                // Сразу включение зон инсоляции
                p.IsVisualIllumsOn = true;
            }
        }        

        /// <summary>
        /// Показать точку на чертеже
        /// </summary>        
        public void ShowPoint (InsPoint selectedPoint)
        {
            if (selectedPoint == null) return;

            var doc = Model.Doc;
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
            VisualPointsOnOff(onOff, true);
            VisualTreeOnOff(onOff, true);            
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
            VisualPointsOnOff(IsVisualIllumsOn, false);
        }

        /// <summary>
        /// Включение/выключение визуализации зон инсоляции точек
        /// <param name="onOff">Null - по состоянию в точке, иначе принудительно</param>
        /// </summary>        
        private void VisualPointsOnOff (bool onOff, bool saveState)
        {
            // Включение/выключение визуализации инсоляции точек
            if (Points != null)
            {
                foreach (var item in Points)
                {
                    // Изменение состояние на заданное                    
                    item.VisualOnOff(onOff, saveState);
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
                VisualTrees.VisualIsOn = onOff ? IsVisualTreeOn : false;
            }
            else
            {
                VisualTrees.VisualIsOn = onOff;
            }
        }

        private void OnIsVisualTreeOnChanged()
        {
            VisualTrees.VisualIsOn = IsVisualTreeOn;
        }

        /// <summary>
        /// Проверка есть ли уже такая точка в списке точек инсоляции
        /// </summary>        
        /// <param name="dubl">Дублируется ли эта точка - т.е. если такая точка только одна, то ок, а две и больше - дубликаты</param>
        public bool HasPoint (Point3d pt, bool dubl = false)
        {            
            if (dubl)
            {
                return Points.Where(p => p.Point.IsEqualTo(pt, tolerancePoints)).Skip(1).Any();
            }
            else
            {
                return Points.Any(p => p.Point.IsEqualTo(pt, tolerancePoints));
            }            
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
                }
            }
            VisualTrees.VisualUpdate();
        }

        private void CheckPoints ()
        {
            // Проверка точек                      
            
        }
    }
}
