using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.UI;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Модель инсоляции в привязке к документу
    /// </summary>
    public class InsModel : ModelBase, ITypedDataValues, IDisposable
    {
        /// <summary>
        /// Для восстановление сохраненного расчета инсоляции
        /// Пока не реализовано
        /// </summary>
        public InsModel () { }

        /// <summary>
        /// Создание модели расчета из списка значений считанных из словаря чертежа
        /// </summary>
        /// <param name="values"></param>
        /// <param name="doc"></param>
        public InsModel (List<TypedValue> values, Document doc)
        {            
            Doc = doc;            
        }

        /// <summary>
        /// Флаг - требуется обновление расчета
        /// </summary>        
        public bool IsUpdateRequired { get; set; }        
        public Document Doc { get; set; }        
        public Map Map { get; set; }        
        public ICalcService CalcService { get; set; }
        /// <summary>
        /// Настройки инсоляции
        /// </summary>
        public InsOptions Options { get { return options; } set { options = value; RaisePropertyChanged(); } }
        InsOptions options;
        /// <summary>
        /// Расчет елочек
        /// </summary>
        public TreeModel Tree { get { return tree; } set { tree = value; RaisePropertyChanged(); } }
        TreeModel tree;    
        /// <summary>
        /// Расчет фронтонов
        /// </summary>
        public FrontModel Front { get { return front; } set { front = value; RaisePropertyChanged(); } }
        FrontModel front;
        /// <summary>
        /// Расчет площадок
        /// </summary>
        public PlaceModel Place { get { return place; } set { place = value; RaisePropertyChanged(); } }
        PlaceModel place;

        public string UpdateInfo { get; set; } = "Обновление расчета";
        /// <summary>
        /// Состояние - включен/отключен расчет
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Общие действия и при создании нового расчета и при загрузке существующего
        /// Обязательно запускать после создания расчета
        /// </summary>
        public void Initialize (Document doc)
        {
            Doc = doc;

            // Дефолтные настройки
            if (Options == null)
                Options = InsOptions.Default();

            // Загрузка карты
            if (Map != null)
            {
                // ??? пока ничего                
                Map.Update();
            }
            else
            {
                Map = new Map(Doc);
                Map.BuildingAdded += Map_BuildingAdded;
                Map.BuildingErased += Map_BuildingErased;
                Map.BuildingModified += Map_BuildingModified;
                Map.InsPointAdded += Map_InsPointAdded;
            }            

            // Создание расчета елочек
            if (Tree == null)
            {
                Tree = new TreeModel();
            }
            Tree.Initialize(this);

            // Расчет фронтов
            if (Front== null)
            {
                Front = new FrontModel();
            }
            Front.Initialize(this);

            // Расчет площадок
            if (Place == null)
            {
                Place = new PlaceModel();
            }
            Place.Initialize(this);

            doc.Database.BeginSave += Database_BeginSave;
            Redrawable();            
        }

        public void Redrawable ()
        {
            // ????
        }

        private void Database_BeginSave (object sender, Autodesk.AutoCAD.DatabaseServices.DatabaseIOEventArgs e)
        {
            // При сохранении чертежа - сохранение расчета инсоляции
            try
            {
                SaveIns();
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Изменение 
        /// </summary>
        /// <param name="building"></param>
        public void ChangeBuildingType (MapBuilding building)
        {            
            var pointsInBuilding = Tree.GetPointsInBuilding(building);
            foreach (var item in pointsInBuilding)
            {
                item.Update();
            }
        }

        /// <summary>
        /// Обновление всех расчетов
        /// </summary>
        public void Update(Document doc = null)
        {
            if (doc == null)
            {
                Doc = Application.DocumentManager.MdiActiveDocument;
            }
            else
            {
                Doc = doc;
            }

            // Заново инициализация
            Initialize(Doc);

            // Сервис расчета            
            DefineCalcService();

            // Загрузка точек всех типов и добавление в расчеты
            LoadPoints();

            Front.Update();

            Place.Update();

            IsUpdateRequired = false;
            UpdateInfo = "Обновление расчета";

            // Перерисовка точек
            Redrawable();            
        }

        /// <summary>
        /// Обновление визуализаций
        /// </summary>
        public void UpdateVisual ()
        {
            Tree?.UpdateVisual();
            Front?.UpdateVisual();
        }

        /// <summary>
        /// Определение расчетного сервиса по Options
        /// </summary>
        /// <returns>Изменилось или нет</returns>
        public bool DefineCalcService ()
        {
            bool res = false;
            if (CalcService == null || !CalcService.IsIdenticalOptions (Options))
            { 
                CalcService = InsService.GetCalcService(Options);
                res = true;
            }
            return res;
        }        

        /// <summary>
        /// Сохранение расчета в словарь чертежа
        /// </summary>
        public void SaveIns ()
        {
            if (Doc == null || Doc.IsDisposed) return;
            // Словарь InsModel
            var dicInsModel = new DicED("InsModel");
            // Список значений самого расчета InsModelRec                        
            dicInsModel.AddRec("InsModelRec", GetDataValues(Doc));   
            // Словарь настроек InsOptions            
            dicInsModel.AddInner("InsOptions", Options.GetExtDic(Doc));
            // Словарь расчета елочек TreeModel            
            dicInsModel.AddInner("TreeModel", Tree.GetExtDic(Doc));
            // Словарь расчета фронтов FrontModel            
            dicInsModel.AddInner("FrontModel", Front.GetExtDic(Doc));
            // Словарь расчета площадок
            dicInsModel.AddInner("PlaceModel", Place.GetExtDic(Doc));
            // Сохранение словаря InsModel в NOD чертежа
            InsExtDataHelper.SaveToNod(Doc, dicInsModel);

            // Сохранение всех точек
            Tree.SavePoints();            
        }

        /// <summary>
        /// Загрузка расчета из документа (если он был там сохранен)
        /// Без инициализации!!!
        /// </summary>
        /// <param name="doc">Документ</param>
        /// <returns>Расчет инсоляции или null</returns>
        public static InsModel LoadIns (Document doc)
        {
            InsModel model = null;

            // Загрузка словаря модели
            var dicModel = InsExtDataHelper.LoadFromNod(doc, "InsModel");
            if (dicModel == null) return model;

            // список значений самой модели            
            var recModel = dicModel.GetRec("InsModelRec");
            // Настройки
            var opt = new InsOptions();
            opt.SetExtDic(dicModel.GetInner("InsOptions"), doc);
            // Расчет елочек
            var tree = new TreeModel();
            tree.SetExtDic(dicModel.GetInner("TreeModel"), doc);
            // Расчет фронтов
            var front = new FrontModel();            
            front.SetExtDic(dicModel.GetInner("FrontModel"), doc);
            // Расчет площадок
            var place = new PlaceModel();
            place.SetExtDic(dicModel.GetInner("PlaceModel"), doc);

            model = new InsModel();
            model.Doc = doc;
            model.SetDataValues(recModel?.Values, doc);
            model.Options = opt;
            model.Tree = tree;
            model.Front = front;
            model.Place = place;
            //model.Initialize(doc);           

            return model;
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            // Пока нет значений для сохранения
            return null;
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
        }

        /// <summary>
        /// Поис инс точки среди всех точк расчета
        /// </summary>        
        public IInsPoint FindInsPoint (Point3d pt)
        {
            var res = Tree.Points?.FirstOrDefault(p => p.Point == pt);
            return res;
        }

        /// <summary>
        /// Загрузка точек. Определение точек найденных на карте
        /// </summary>        
        private void LoadPoints ()
        {
            var doc = Doc;
            if (doc == null) return;

            Tree.ClearVisuals();
            Tree.Points.Clear();

            var idPoints = Map.InsPoints;
            if (idPoints == null || idPoints.Count == 0)
                return;

            foreach (var idPt in idPoints)
            {
                DefinePoint(idPt);
            }

            Tree.UpdateVisualTree();
        }
        

        private void DefinePoint (ObjectId idPt)
        {
            DicED dicPt;
            using (var t = Doc.TransactionManager.StartTransaction())
            {
                var dbPt = idPt.GetObject(OpenMode.ForRead) as DBPoint;
                if (dbPt == null) return;

                // Загрузка из словаря всех записей
                dicPt = InsExtDataHelper.Load(dbPt, Doc);
                t.Commit();
            }

            // Если это инсоляционная точка елочек
            var dicInsPt = dicPt?.GetInner("InsPoint");
            if (dicInsPt != null)
            {
                Tree.AddPoint(dicInsPt, idPt);
            }
        }

        /// <summary>
        /// Очистка визуализаций (перед переключением на другой чертеж)
        /// </summary>
        public void ClearVisual ()
        {
            Map.ClearVisual();
            Tree.ClearVisuals();
            Front.ClearVisual();
            Place.ClearVisual();        
        }

        private void Map_BuildingModified (object sender, MapBuilding e)
        {
            // Флаг что расчет требуется обновить - т.к. изменились здания на чертеже
            IsUpdateRequired = true;
            UpdateInfo = "Требуется обновление - изменено здание.";            
            //Update();
        }        

        private void Map_BuildingErased (object sender, MapBuilding e)
        {
            // Флаг что расчет требуется обновить - т.к. изменились здания на чертеже
            IsUpdateRequired = true;
            UpdateInfo = "Требуется обновление - удалено здание.";
            //Update();
        }

        private void Map_BuildingAdded (object sender, MapBuilding e)
        {
            // Флаг что расчет требуется обновить - т.к.изменились здания на чертеже
            IsUpdateRequired = true;
            UpdateInfo = "Требуется обновление - добавлено здание.";
            //Update();
        }

        private void Map_InsPointAdded (object sender, ObjectId e)
        {
            IsUpdateRequired = true;
            UpdateInfo = "Требуется обновление - добавлена расчетная точка.";

            // Определение типа точки и добавление в соответствующий расчет
            //LoadPoint(e);
        }

        public void Dispose ()
        {
            if (Doc == null || Doc.IsDisposed) return;
            using (Doc.LockDocument())
            {
                Tree.Dispose();
                Map.Dispose();
                Doc.Database.BeginSave -= Database_BeginSave;
            }
            Doc = null;
        }
    }
}
