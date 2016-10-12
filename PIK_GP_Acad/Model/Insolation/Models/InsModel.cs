using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
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
    public class InsModel : SavableModelBase<InsModel>
    {
        /// <summary>
        /// Для восстановление сохраненного расчета инсоляции
        /// Пока не реализовано
        /// </summary>
        public InsModel () { }
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
            Map = new Map(this);
            Map.BuildingAdded += Map_BuildingAdded;
            Map.BuildingErased += Map_BuildingErased;
            Map.BuildingModified += Map_BuildingModified;
            Map.InsPointAdded += Map_InsPointAdded;

            // Сервис расчета
            if (CalcService == null)
                DefineCalcService();            

            // Создание расчета
            if (Tree == null)
                Tree = new TreeModel(this);

            // Загрузка точек из найденных на карте
            if (Map.InsPoints.Any())
            {
                LoadPoints(Map.InsPoints);
            }            
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
        /// Обновление всех расчетов
        /// </summary>
        public void Update()
        {            
            Tree.Update();
            IsUpdateRequired = false;
            UpdateInfo = "Обновление расчета";
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

        public void SaveIns ()
        {
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
        /// </summary>
        /// <param name="doc">Документ</param>
        /// <returns>Расчет инсоляции или null</returns>
        public static InsModel LoadIns (Document doc)
        {
            InsModel res = null;
            //try
            //{
            //    using (var fileStream = File.Open(@"\\picompany.ru\root\dep_ort\8.САПР\проекты\AutoCAD\РГ\ГП\Концепция\Инсоляция\Расчет в точке\insModel.xml", FileMode.Open))
            //    {
            //        res = Load<InsModel>(fileStream, SerializationMode.Xml, new SerializationConfiguration());                    
            //    }                
            //}
            //catch { }

            return res;
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
        /// Загрузка точек. Определение точек и добавление их в соответствующие расчеты
        /// </summary>
        /// <param name="idsPoint">Список инсоляционных найденных точек на чертеже</param>
        private void LoadPoints (List<ObjectId> idsPoint)
        {
            using (Doc.LockDocument())
            using (var t = Doc.TransactionManager.StartTransaction())
            {
                foreach (var idPt in idsPoint)
                {
                    var dbPt = idPt.GetObject(OpenMode.ForRead) as DBPoint;
                    if (dbPt == null) continue;

                    InsPoint insPoint = null;

                    // Загрузка из словаря всех записей
                    var records = InsExtDataHelper.Load(dbPt, Doc);

                    List<TypedValue> values;
                    // Если это инсоляционная точка елочек
                    if (records.TryGetValue(InsPoint.DataRec, out values))
                    {
                        insPoint = new InsPoint(values, dbPt, this);
                        // Добавление точки в расчет елочек
                        Tree.AddPoint(insPoint);
                    }                                        
                }
                t.Commit();
            }
        }

        private void Map_BuildingModified (object sender, InsBuilding e)
        {
            // Флаг что расчет требуется обновить - т.к. изменились здания на чертеже
            IsUpdateRequired = true;
            UpdateInfo = "Требуется обновление - изменено здание.";
        }

        private void Map_BuildingErased (object sender, InsBuilding e)
        {
            // Флаг что расчет требуется обновить - т.к. изменились здания на чертеже
            IsUpdateRequired = true;
            UpdateInfo = "Требуется обновление - удалено здание.";
        }

        private void Map_BuildingAdded (object sender, InsBuilding e)
        {
            // Флаг что расчет требуется обновить - т.к.изменились здания на чертеже
            IsUpdateRequired = true;
            UpdateInfo = "Требуется обновление - добавлено здание.";
        }

        private void Map_InsPointAdded (object sender, ObjectId e)
        {
            IsUpdateRequired = true;
            UpdateInfo = "Требуется обновление - добавлена расчетная точка.";
        }
    }
}
