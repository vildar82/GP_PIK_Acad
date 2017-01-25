using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using PIK_GP_Acad.Insolation.Services;
using AcadLib;
using Autodesk.AutoCAD.Colors;
using MicroMvvm;

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
        
        public InsModel Model { get; set; }        
        public Point3d Point { get; set; }                                
        public ObjectId DBPointId { get; set; }
        /// <summary>
        /// Визуализирует точку
        /// </summary>                    
        public IVisualService VisualPoint { get; set; }
        public double AngleEndOnPlane { get; set; }
        public double AngleStartOnPlane { get; set; }        
        public MapBuilding Building { get; set; }         
        /// <summary>
        /// Высота расчетной точки (с учетом уровня здания, 1 нежилого этажа и высоте от пола до центра окна!!!???)
        /// </summary>                       
        public double Height { get; set; }  
        public List<IIlluminationArea> Illums { get; set; }        
        public string Info { get { return info; } set { info = value; RaisePropertyChanged(); } }
        string info;
        public InsValue InsValue { get { return insValue; } set { insValue = value; RaisePropertyChanged(); } }
        InsValue insValue;     
        public int Number { get { return number; } set { number = value; RaisePropertyChanged(); } }
        int number;
        public WindowOptions Window { get; set; }       
        /// <summary>
        /// Список значений для сохранения в словарь
        /// </summary>
        /// <returns></returns>
        public abstract List<TypedValue> GetDataValues (Document doc);
        public abstract void SetDataValues (List<TypedValue> values, Document doc);
        public abstract DicED GetExtDic (Document doc);
        public abstract void SetExtDic (DicED DicED, Document doc);
        public abstract void Initialize (TreeModel treeModel);
        
        public abstract void VisualOnOff (bool onOff, bool saveState);

        /// <summary>
        /// Создание точки на чертеже
        /// </summary>
        public void CreatePoint()
        {            
            var doc = Model.Doc;
            // Создать точку на чертеже и записать в нее xdata и dictionary
            if (DBPointId.IsNull)
            {
                using (doc.LockDocument())
                using (var t = doc.TransactionManager.StartTransaction())
                {
                    // Если точка уже не была добавлена в чертеж                
                    Model.Map.IsEventsOn = false;
                    var dbPoint = new DBPoint(Point);
                    var cs = doc.Database.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                    DBPointId = cs.AppendEntity(dbPoint);
                    SubscribeDbo(dbPoint);
                    t.AddNewlyCreatedDBObject(dbPoint, true);
                    InsPointHelper.SetInsPoint(dbPoint);
                    t.Commit();                    
                }
                Model.Map.IsEventsOn = true;
            }         
        }

        /// <summary>
        /// Перерисовка описания точки
        /// </summary>
        public void Redrawable ()
        {
            //???            
        }

        public virtual void Update() {
                        
        }

        private void SubscribeDbo (DBPoint dbPoint)
        {            
            dbPoint.Erased += DbPoint_Erased;            
            dbPoint.Modified += DbPoint_Modified;
        }
        private void UnSubscribeDbo (DBPoint dbPoint)
        {
            dbPoint.Erased -= DbPoint_Erased;            
            dbPoint.Modified -= DbPoint_Modified;
        }

        public ObjectId GetDBObject ()
        {
            return DBPointId;
        }

        /// <summary>
        /// Очистка  - отключение
        /// </summary>
        public virtual void ClearVisual()
        {            
            var dbPt = DBPointId.GetObject(OpenMode.ForRead) as DBPoint;
            UnSubscribeDbo(dbPt);
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
                DefineBuilding(true);
                // Обновление точки                
                Update();
            }            
        }       

        private void DbPoint_Erased (object sender, ObjectErasedEventArgs e)
        {
            // Удаление точки из расчета. Проверка есть ли такая точка в расчете (если нет, то она уже удалена через палитру)            
            if (Model.Tree.HasPoint(e.DBObject.Id))
            {
                Delete();
            }            
        }

        public virtual void Delete ()
        {
            // Удаление точки с чертежа
            if (DBPointId.IsNull) return;
            var doc = Model.Doc;
            using (doc.LockDocument())
            {             
                if (DBPointId.IsValidEx())
                {
                    var dbPt = DBPointId.Open(OpenMode.ForWrite, false, true);
                    dbPt.Erase();
                }                
            }
        }

        private void OnNumberChanged ()
        {
            VisualPoint?.VisualUpdate();
        }

        public void DefineBuilding (bool isAlreadyAddedPoint)
        {
            var pt = Point;
            var building = DefineBuilding(ref pt, Model);
            if (building != null)
            {
                // Проверка не дублируется ли эта точка
                if (Model.Tree.HasPoint(pt, isAlreadyAddedPoint))
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
        public static MapBuilding DefineBuilding (ref Point3d point, InsModel model)
        {
            var pt = point;
            var building = model.Map.GetBuildingInPoint(pt);
            if (building != null)
            {
                // Проверка находится ли точка на контуре дома
                if (!CorrectCalcPoint(ref pt, building, model.Doc))
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

        public static bool CorrectCalcPoint (ref Point3d pt, MapBuilding building, Document doc)
        {
            bool res;
            // Корректировка точки
            Point3d correctPt;
            building.InitContour();
            using (building.Contour)
            {
                correctPt = building.Contour.GetClosestPointTo(pt, true);
            }

            if ((pt - correctPt).Length < 0.3)
            {                
                // Точка достаточно близко к контуру - поправка точки и ОК.
                pt = correctPt;
                res = true;

            }
            else
            {
                // Точка далеко от контура - не пойдет.
                res = false;
#if TEST
                //building.InitContour();
                //using (building.Contour)
                //{
                //    EntityHelper.AddEntityToCurrentSpace((Autodesk.AutoCAD.DatabaseServices.Polyline)building.Contour.Clone());
                //}
                //var dbPt = new DBPoint(correctPt);
                //dbPt.Color = Color.FromColor(System.Drawing.Color.AliceBlue);
                //EntityHelper.AddEntityToCurrentSpace(dbPt);
                //dbPt = new DBPoint(pt);
                //dbPt.Color = Color.FromColor(System.Drawing.Color.Gold);
                //EntityHelper.AddEntityToCurrentSpace(dbPt);
#endif
            }
            return res;
        }

        public virtual void Dispose ()
        {
            
        }
    }
}
