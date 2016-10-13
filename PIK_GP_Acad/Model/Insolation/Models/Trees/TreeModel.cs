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
using Autodesk.AutoCAD.ApplicationServices;
using AcadLib.XData;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчет елочек
    /// </summary>
    [Serializable]
    public class TreeModel : ModelBase, INodDataSave, ITypedDataValues
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
        /// Инициализация расчета елочек - новая или обновление старого
        /// </summary>        
        public void Initialize(InsModel insModel)
        {
            this.Model = insModel;

            // Визуализация елочек
            if (VisualTrees != null)
            {
                // Удаление старой визуализации
                VisualTrees.VisualsDelete();                
            }
            VisualTrees = new VisualTree(insModel);

            // Расчетные точки
            if (Points == null)
            {
                Points = new ObservableCollection<InsPoint>();
                VisualTrees.Points = Points;
            }
            else
            {
                // Очистка точек и новая загрузка
                LoadPoints();
            }
            
            if (TreeOptions == null)
                TreeOptions = TreeOptions.Default();
        }         

        /// <summary>
        /// Расчетные точки
        /// </summary>
        [ExcludeFromSerialization]
        public ObservableCollection<InsPoint> Points { get; private set; }
                
        /// <summary>
        /// Настройки елочек
        /// </summary>
        public TreeOptions TreeOptions { get; set; }             

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
                // Обновление - Расчет и визуализация точки         
                p.Update();                
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

        /// <summary>
        /// Проверка - есть ли точка с таким id в расчете
        /// </summary>        
        public bool HasPoint (ObjectId idPoint)
        {
            return Points.Any(p => p.DBPointId == idPoint);
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

        /// <summary>
        /// Очистка - отключение расчета
        /// </summary>
        public void Clear ()
        {            
            foreach (var item in Points)
            {
                item.Clear();
            }
        }

        private void CheckPoints ()
        {
            // Проверка точек   
        }        

        public DicED GetExtDic (Document doc)
        {
            DicED dicTree = new DicED();

            // Список значений расчета елочек                  
            var recTree = new RecXD("TreeModelRec", GetDataValues(doc));
            dicTree.AddRec(recTree);

            // Сохранение настроек елочек
            var dicTreeOptions = TreeOptions.GetExtDic(doc);
            dicTreeOptions.Name = "TreeOptions";
            dicTree.AddInner(dicTreeOptions);

            return dicTree;
        }

        /// <summary>
        /// Установка значений из словаря.
        /// Расчетная модель еще не задана!!!
        /// </summary>        
        public void SetExtDic (DicED dicTree, Document doc)
        {
            // Собственные значения рассчета елочек
            var recTree = dicTree.GetRec("TreeModelRec");            
            SetDataValues(recTree.Values, doc);

            // настроки елочек            
            var dicTreeOpt = dicTree.GetInner("TreeOptions");
            TreeOptions = new TreeOptions();
            TreeOptions.SetExtDic(dicTreeOpt, doc);
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            return new List<TypedValue>() {
                new TypedValue((int)DxfCode.ExtendedDataInteger32, IsVisualIllumsOn? 1:0),
                new TypedValue((int)DxfCode.ExtendedDataInteger32, IsVisualTreeOn? 1:0),
            };            
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            if (values == null || values.Count != 2)
            {
                // Default
                IsVisualIllumsOn = false;
                IsVisualTreeOn = false;
            }
            else
            {
                int index = 0;
                IsVisualIllumsOn = values[index++].GetTvValue<int>() == 0 ? false : true;
                IsVisualTreeOn = values[index++].GetTvValue<int>() == 0 ? false : true;
            }
        }

        /// <summary>
        /// Загрузка точек. Определение точек найденных на карте
        /// </summary>        
        private void LoadPoints ()
        {
            var doc = Model?.Doc;
            if (doc == null) return;

            Points.Clear();

            var idPoints = Model.Map.InsPoints;
            if (idPoints == null || idPoints.Count ==0)            
                return;            
            

            using (doc.LockDocument())
            using (var t =doc.TransactionManager.StartTransaction())
            {
                foreach (var idPt in idPoints)
                {
                    var dbPt = idPt.GetObject(OpenMode.ForRead) as DBPoint;
                    if (dbPt == null) continue;

                    InsPoint insPoint = null;

                    // Загрузка из словаря всех записей
                    var records = InsExtDataHelper.Load(dbPt, doc);

                    List<TypedValue> values;
                    // Если это инсоляционная точка елочек
                    if (records.TryGetValue(InsPoint.DataRec, out values))
                    {
                        insPoint = new InsPoint(values, dbPt, Model);
                        // Добавление точки в расчет елочек
                        AddPoint(insPoint);
                    }
                }
                t.Commit();
            }
        }
    }
}
