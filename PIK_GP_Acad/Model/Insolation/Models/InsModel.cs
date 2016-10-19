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
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.Runtime.Serialization;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.UI;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Модель инсоляции в привязке к документу
    /// </summary>
    public class InsModel : ModelBase, ITypedDataValues
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

        ///// <summary>
        ///// Создание нового расчета инсоляции для документа
        ///// </summary>        
        //public InsModel (Document doc): base()
        //{
        //    Doc = doc;

        //}

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
                Map = new Map(this);
                Map.BuildingAdded += Map_BuildingAdded;
                Map.BuildingErased += Map_BuildingErased;
                Map.BuildingModified += Map_BuildingModified;
                Map.InsPointAdded += Map_InsPointAdded;
            }

            // Сервис расчета            
            DefineCalcService();

            // Создание расчета
            if (Tree == null)
            {
                Tree = new TreeModel();
            }
            Tree.Initialize(this);

            // Загрузка точек всех типов и добавление в расчеты
            LoadPoints();

            Tree.UpdateVisualTree();

            doc.Database.BeginSave += Database_BeginSave;
            Redrawable();
        }

        public void Redrawable ()
        {
            // ????
        }

        /// <summary>
        /// Флаг - требуется обновление расчета
        /// </summary>
        [ExcludeFromSerialization]
        public bool IsUpdateRequired { get; set; }

        [ExcludeFromSerialization]
        public Document Doc { get; set; }

        [ExcludeFromSerialization]
        public Map Map { get; set; }

        [ExcludeFromSerialization]        
        public IInsCalcService CalcService { get; set; }
        /// <summary>
        /// Настройки инсоляции
        /// </summary>
        public InsOptions Options { get; set; }
        /// <summary>
        /// Расчет елочек
        /// </summary>
        public TreeModel Tree { get; set; }
        [ExcludeFromSerialization]
        public string UpdateInfo { get; set; } = "Обновление расчета";

        private void Database_BeginSave (object sender, Autodesk.AutoCAD.DatabaseServices.DatabaseIOEventArgs e)
        {
            // При сохранении чертежа - сохранение расчета инсоляции
            SaveIns();
        }

        /// <summary>
        /// Изменение 
        /// </summary>
        /// <param name="building"></param>
        public void ChangeBuildingType (InsBuilding building)
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
        public void Update()
        {
            // Заново инициализация
            Initialize(Doc);

            IsUpdateRequired = false;
            UpdateInfo = "Обновление расчета";

            // Перерисовка точек
            Redrawable();
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
            // Словарь InsModel
            var DicED = new DicED("InsModel");
            // Список значений самого расчета InsModelRec                        
            DicED.AddRec("InsModelRec", GetDataValues(Doc));   
            // Словарь настроек InsOptions            
            DicED.AddInner("InsOptions", Options.GetExtDic(Doc));
            // Словарь расчета елочек TreeModel            
            DicED.AddInner("TreeModel", Tree.GetExtDic(Doc));
            // Сохранение словаря InsModel в NOD чертежа
            InsExtDataHelper.SaveToNod(Doc, DicED);

            // Сохранение всех точек
            Tree.SavePoints();

            //try
            //{
            //    // серилизация расчета            
            //    using (var fileStream = File.Create(@"\\picompany.ru\root\dep_ort\8.САПР\проекты\AutoCAD\РГ\ГП\Концепция\Инсоляция\Расчет в точке\insModel.xml"))
            //    {
            //        Save(fileStream, SerializationMode.Xml, new SerializationConfiguration());
            //    }
            //}
            //catch { }
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
            InsOptions opt = new InsOptions();
            opt.SetExtDic(dicModel.GetInner("InsOptions"), doc);
            // Расчет елочек
            TreeModel tree = new TreeModel();
            tree.SetExtDic(dicModel.GetInner("TreeModel"), doc);            

            model = new InsModel();
            model.SetDataValues(recModel?.Values, doc);
            model.Options = opt;
            model.Tree = tree;            
            //model.Initialize(doc);

            //try
            //{
            //    using (var fileStream = File.Open(@"\\picompany.ru\root\dep_ort\8.САПР\проекты\AutoCAD\РГ\ГП\Концепция\Инсоляция\Расчет в точке\insModel.xml", FileMode.Open))
            //    {
            //        res = Load<InsModel>(fileStream, SerializationMode.Xml, new SerializationConfiguration());                    
            //    }                
            //}
            //catch { }

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
            var res = Tree.Points.FirstOrDefault(p => p.Point == pt);
            return res;
        }

        /// <summary>
        /// Загрузка точек. Определение точек найденных на карте
        /// </summary>        
        private void LoadPoints ()
        {
            var doc = Doc;
            if (doc == null) return;

            var idPoints = Map.InsPoints;
            if (idPoints == null || idPoints.Count == 0)
                return;

            //using (doc.LockDocument())
            using (var t = doc.TransactionManager.StartTransaction())
            {                
                foreach (var idPt in idPoints)
                {
                    DefinePoint(idPt);
                }
                t.Commit();
            }
        }

        private void LoadPoint (ObjectId idPt)
        {
            //using (Doc.LockDocument())
            using (var t = Doc.TransactionManager.StartTransaction())
            {
                DefinePoint(idPt);
                t.Commit();
            }
        }

        private void DefinePoint(ObjectId idPt)
        {
            var dbPt = idPt.GetObject(OpenMode.ForRead) as DBPoint;
            if (dbPt == null) return;

            // Загрузка из словаря всех записей
            var dicEd = InsExtDataHelper.Load(dbPt, Doc);

            // Если это инсоляционная точка елочек
            var dicInsPt = dicEd.GetInner("InsPoint");
            if (dicInsPt != null)
            {
                Tree.AddPoint(dicInsPt, idPt);
            }
        }

        /// <summary>
        /// Очистка расчета (отключение расчета)
        /// </summary>
        public void Clear ()
        {
            Map.Clear();
            Tree.Clear();
        }

        private void Map_BuildingModified (object sender, InsBuilding e)
        {
            // Флаг что расчет требуется обновить - т.к. изменились здания на чертеже
            IsUpdateRequired = true;
            UpdateInfo = "Требуется обновление - изменено здание.";            
            //Update();
        }        

        private void Map_BuildingErased (object sender, InsBuilding e)
        {
            // Флаг что расчет требуется обновить - т.к. изменились здания на чертеже
            IsUpdateRequired = true;
            UpdateInfo = "Требуется обновление - удалено здание.";
            //Update();
        }

        private void Map_BuildingAdded (object sender, InsBuilding e)
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
    }
}
