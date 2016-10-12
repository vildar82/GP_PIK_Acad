﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Catel.Data;
using Catel.MVVM;
using Catel.Runtime.Serialization;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Определяет инсоляционную точку (для елочек, фронтонов)
    /// </summary>
    public abstract class InsPointBase : ModelBase, IInsPoint
    {
        public InsPointBase () { }

        /// <summary>
        /// Создание объекта - пользователем
        /// Точка не создается на чертеже (нужно запустить CreatePoint)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="doc"></param>
        public InsPointBase (Point3d point, InsModel model)
        {
            this.Model = model;
            Point = point;
        }

        /// <summary>
        /// Создание расчетной точки из найденной точки на чертеже (DBPoint)
        /// </summary>
        /// <param name="dbPt">Инс точка на чертеже</param>
        /// <param name="model">Модель</param>
        public InsPointBase (DBPoint dbPt, InsModel model)
        {
            this.Model = model;
            Point = dbPt.Position;
            DBPointId = dbPt.Id;
            SubscribeDbo(dbPt);
        }

        [ExcludeFromSerialization]
        public InsModel Model { get; set; }
        [ExcludeFromSerialization]
        public Point3d Point { get; set; }        
        public double[] PointAsXYZ {
            get { return new double[] { Point.X, Point.Y, Point.Z }; }
            set { Point = new Point3d(value); }
        }

        [ExcludeFromSerialization]
        public ObjectId DBPointId { get; set; }

        /// <summary>
        /// Визуализирует точку
        /// </summary>        
        [ExcludeFromSerialization]
        public IVisualService VisualPoint { get; set; }

        public double AngleEndOnPlane { get; set; }
        public double AngleStartOnPlane { get; set; }
        [ExcludeFromSerialization]
        public InsBuilding Building { get; set; }
        public TaskCommand DeletePoint { get; }
        public TaskCommand EditPoint { get; }
        public int Height { get; set; }
        [ExcludeFromSerialization]
        public List<IIlluminationArea> Illums { get; set; }
        [ExcludeFromSerialization]
        public string Info { get; set; }
        [ExcludeFromSerialization]
        public InsValue InsValue { get; set; }
        [ExcludeFromSerialization]
        public int Number { get; set; }
        public WindowOptions Window { get; set; }
        /// <summary>
        /// Назнание для сохранения в словарь расширенных данных
        /// </summary>
        public abstract string DataRecName { get;  }
        /// <summary>
        /// Список значений для сохранения в словарь
        /// </summary>
        /// <returns></returns>
        public abstract List<TypedValue> GetDataValues ();

        /// <summary>
        /// Создание точки на чертеже
        /// </summary>
        public void CreatePoint()
        {
            var doc = Model.Doc;
            // Создать точку на чертеже и записать в нее xdata и dictionary
            using (doc.LockDocument())
            using (var t = doc.TransactionManager.StartTransaction())
            {
                var dbPoint = new DBPoint(Point);
                var cs = doc.Database.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                DBPointId = cs.AppendEntity(dbPoint);
                t.AddNewlyCreatedDBObject(dbPoint, true);
                SubscribeDbo(dbPoint);

                InsPointHelper.SetInsPoint(dbPoint);

                t.Commit();
            }
        }

        private void SubscribeDbo (DBPoint dbPoint)
        {
            dbPoint.Erased += DbPoint_Erased;
            //dbPoint.Copied += DbPoint_Copied; Создание нового объекта отслеживается картой Map
            dbPoint.Modified += DbPoint_Modified;
        }

        public ObjectId GetDBObject ()
        {
            return DBPointId;
        }

        /// <summary>
        /// Перемещение точки на чертеже
        /// </summary>        
        private void DbPoint_Modified (object sender, EventArgs e)
        {
            // Изменение точки  
            // Если точка переместилась по x,y то обновление расчета точки
            var dbPt = (DBPoint)sender;
            if (!dbPt.Position.IsEqualTo(Point))
            {
                Point = dbPt.Position;
                // Определение здания
                DefineBuilding();
                // Обновление точки                
                Update();
            }            
        }       

        private void DbPoint_Erased (object sender, ObjectErasedEventArgs e)
        {
            // Удаление точки из расчета  
            Delete();          
        }

        public abstract void Initialize (TreeModel treeModel);
        public abstract void Update ();
        public abstract void VisualOnOff (bool onOff, bool saveState);
        public virtual void Delete ()
        {
            // Удаление точки с чертежа
            if (DBPointId.IsNull) return;
            var doc = Model.Doc;
            using (doc.LockDocument())
                using (var t = doc.TransactionManager.StartTransaction())
            {
                var dbPt = DBPointId.GetObject(OpenMode.ForWrite);
                dbPt.Erase();
                t.Commit();
            }
        }

        private void OnNumberChanged ()
        {
            VisualPoint?.VisualUpdate();
        }

        public void DefineBuilding ()
        {
            var pt = Point;
            var building = DefineBuilding(ref pt, Model);
            if (building != null)
            {
                // Проверка не дублируется ли эта точка
                if (Model.Tree.HasPoint(pt, true))
                {
                    building = null;
                }
                else
                {
                    Point = pt;
                }
            }
            Building = building;
        }

        /// <summary>
        /// Определение здания которому принадлежит эта точка
        /// </summary>
        public static InsBuilding DefineBuilding (ref Point3d point, InsModel model)
        {
            var pt = point;
            var building = model.Map.GetBuildingInPoint(pt);
            if (building != null)
            {
                // Проверка находится ли точка на контуре дома
                if (!CorrectCalcPoint(ref pt, building, model.Doc.Database))
                {
                    building = null;
                }
                else
                {
                    point = pt;                    
                }
            }
            return building;
        }

        public static bool CorrectCalcPoint (ref Point3d pt, InsBuilding building, Database db)
        {
            bool res;
            // Корректировка точки
            Point3d correctPt;
            using (var t = db.TransactionManager.StartTransaction())
            {
                building.InitContour();
                correctPt = building.Contour.GetClosestPointTo(pt, true);
                t.Commit();
            }

            if ((pt - correctPt).Length < 1)
            {
                // Точка достаточно близко к контуру - поправка точки и ОК.
                pt = correctPt;
                res = true;
            }
            else
            {
                // Точка далеко от контура - не пойдет.
                res = false;
            }
            return res;
        }
    }
}
