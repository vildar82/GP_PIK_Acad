using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
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
        /// <summary>
        /// Создание нового расчета инсоляции для документа
        /// </summary>        
        public InsModel (Document doc): base()
        {
            Doc = doc;
            // Дефолтные настройки
            Options = InsOptions.Default();
        }

        public bool IsInsActivated { get; set; }

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

        private void Initialize (Document doc)
        {
            Doc = doc;
            if (IsInsActivated)
                OnIsInsActivatedChanged();
        }

        private void OnIsInsActivatedChanged()
        {
            if (IsInsActivated)
            {
                if (Map == null)                                    
                    Map = new Map(Doc);
                if (Tree == null)
                    Tree = new TreeModel(this);
                else
                    Tree.Initialize(this);
                DefineCalcService();
                Doc.Database.BeginSave += Database_BeginSave;                
            }
            else
            {
                // Отключить инсоляцию для этого документа (всю визуализацию)                
                Tree.VisualsOnOff(false);
                Doc.Database.BeginSave -= Database_BeginSave;
            }
        }

        private void Database_BeginSave (object sender, Autodesk.AutoCAD.DatabaseServices.DatabaseIOEventArgs e)
        {
            try
            {
                SaveIns();
            }
            catch(Exception ex)
            {

            }
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
            // серилизация расчета            
            using (var fileStream = File.Create(@"\\picompany.ru\root\dep_ort\8.САПР\проекты\AutoCAD\РГ\ГП\Концепция\Инсоляция\Расчет в точке\insModel.xml"))
            {
                Save(fileStream, SerializationMode.Xml, new SerializationConfiguration());
            }
        }

        public static InsModel LoadIns (Document doc)
        {
            InsModel res = null;
            using (var fileStream = File.Open(@"\\picompany.ru\root\dep_ort\8.САПР\проекты\AutoCAD\РГ\ГП\Концепция\Инсоляция\Расчет в точке\insModel.xml", FileMode.Open))
            {
                res = Load<InsModel>(fileStream, SerializationMode.Xml, new SerializationConfiguration());
                
            }

            if (res != null)
            {
                // Инициализация объекта
                res.Initialize(doc);
            }

            return res;
        }        
    }
}
