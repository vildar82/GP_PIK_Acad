using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Catel.Data;
using Catel.MVVM;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.UI;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Модель инсоляции в привязке к документу
    /// </summary>
    public class InsModel : ModelBase
    {
        private bool isInsActivated;         

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
        }

        public bool IsInsActivated {
            get { return isInsActivated; }
            set {
                if (value != isInsActivated)
                {
                    ActivateIns(value);                    
                }
                isInsActivated = value;
            }
        }        

        public Document Doc { get; set; }
        public Map Map { get; set; }
        [XmlIgnore]
        public IInsCalcService CalcService { get; set; }
        /// <summary>
        /// Настройки инсоляции
        /// </summary>
        public InsOptions Options { get; set; }
        /// <summary>
        /// Расчет елочек
        /// </summary>
        public TreeModel Tree { get; set; }

        protected override void OnInitialized ()
        {
            base.OnInitialized();            
        }

        private void ActivateIns (bool value)
        {
            if (value)
            {
                if (Map == null)
                {
                    // Дефолтные настройки
                    Options = new InsOptions();
                    Map = new Map(Doc);
                    Tree = new TreeModel(this);
                    CalcService = InsService.GetCalcService(Options);
                }
                // Включить визуализацию
            }
            else
            {
                // Отключить инсоляцию для этого документа (всю визуализацию)                
            }            
        }
    }
}
