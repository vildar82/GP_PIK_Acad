﻿using System;
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
    public class TreeModel : ModelBase, IExtDataSave, ITypedDataValues
    {
        private static Tolerance tolerancePoints = new Tolerance(1, 1);
        private bool isVisualTreeOnOffForLoad;

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
        public void Initialize (InsModel insModel)
        {
            this.Model = insModel;

            // Визуализация елочек
            if (VisualTrees != null)
            {
                // Удаление старой визуализации
                VisualTrees.VisualsDelete();
                VisualTrees.Model = insModel;
            }
            else
            {
                VisualTrees = new VisualTree(insModel);
                if (isVisualTreeOnOffForLoad)
                    VisualTrees.VisualIsOn = isVisualTreeOnOffForLoad;
            }

            // Расчетные точки                            
            //LoadPoints(); // все точки грузятся из InsModel

            if (TreeOptions == null)
                TreeOptions = TreeOptions.Default();

            if (Points != null)
            {
                // Очистка точек, с очисткой визуалз
                DeletePointsVisualIllums();
                Points.Clear();
            }
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

        public void Redrawable ()
        {
            //????
            //foreach (var item in Points)
            //{
            //    item.Redrawable();
            //}
        }

        /// <summary>
        /// Обновление полное рачета елочек
        /// </summary>
        public void Update ()
        {
            foreach (var item in Points)
            {
                item.DefineBuilding(true);
                item.Update();
            }
            UpdateVisualTree(null);
        }        

        /// <summary>
        /// Обновление визуализации елочек
        /// </summary>        
        public void UpdateVisualTree (InsPoint insPoint = null)
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
                if (p.Building == null) p.DefineBuilding(false);
                p.CreatePoint();
                Points.Add(p);       
                // Обновление - Расчет и визуализация точки         
                p.Update();                
            }
        }        

        public void AddPoint (DicED dicInsPt, ObjectId idPt)
        {            
            if (dicInsPt == null || idPt.IsNull) return;

            var dbPt = idPt.GetObject(OpenMode.ForRead) as DBPoint;
            if (dbPt == null) return;

            if (Points == null)
            {
                Points = new ObservableCollection<InsPoint>();                
            }            
            InsPoint insPoint = null;
            insPoint = new InsPoint(dbPt, Model);
            insPoint.SetExtDic(dicInsPt, Model.Doc);
            // Добавление точки в расчет елочек
            AddPoint(insPoint);
        }

        private void DeletePointsVisualIllums ()
        {
            if (Points == null) return;
            // Очистка визуализации точек
            foreach (var item in Points)
            {
                item.VisualIllums?.VisualsDelete();
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
        /// Сохранение точек в чертеже
        /// </summary>
        public void SavePoints ()
        {
            foreach (var item in Points)
            {
                item.SaveInsPoint();
            }
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
            if (VisualTrees != null)
            {
                VisualTrees.VisualIsOn = IsVisualTreeOn;
            }
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
            VisualTrees.VisualsDelete();
            using (var t = Model.Doc.TransactionManager.StartTransaction())
            {
                foreach (var item in Points)
                {
                    item.Clear();
                }
                t.Commit();
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
            if (dicTree == null)
            {
                // Default
                TreeOptions = TreeOptions.Default();
                return;
            }
            // Собственные значения рассчета елочек            
            SetDataValues(dicTree.GetRec("TreeModelRec")?.Values, doc);
            // настроки елочек                        
            TreeOptions = new TreeOptions();
            TreeOptions.SetExtDic(dicTree.GetInner("TreeOptions"), doc);
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            return new List<TypedValue>() {
                TypedValueExt.GetTvExtData(IsVisualIllumsOn),
                TypedValueExt.GetTvExtData(IsVisualTreeOn),
                TypedValueExt.GetTvExtData(VisualTrees.VisualIsOn)                
            };            
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            if (values == null || values.Count != 3)
            {
                // Default
                IsVisualIllumsOn = false;
                IsVisualTreeOn = false; 
                // Елочки дефолтно выключены
            }
            else
            {
                int index = 0;
                IsVisualIllumsOn = values[index++].GetTvValue<bool>();
                IsVisualTreeOn = values[index++].GetTvValue<bool>();
                isVisualTreeOnOffForLoad = values[index++].GetTvValue<bool>();
            }
        }
    }
}
