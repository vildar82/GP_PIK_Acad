using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Catel.Data;
using Catel.MVVM;
using Catel.Runtime.Serialization;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.UI;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Модель инсоляции в привязке к документу
    /// </summary>
    public class InsModel : ModelBase
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

        protected override void OnInitialized ()
        {
            base.OnInitialized();            
        }

        private void OnIsInsActivatedChanged()
        {
            if (IsInsActivated)
            {
                if (Map == null)
                {
                    // Дефолтные настройки
                    Options = new InsOptions();
                    Map = new Map(Doc);
                    Tree = new TreeModel(this);
                    DefineCalcService();                    
                }
                // Включить визуализацию
                Tree.VisualsOnOff(true);
            }
            else
            {
                // Отключить инсоляцию для этого документа (всю визуализацию)                
                Tree.VisualsOnOff(false);
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
    }
}
