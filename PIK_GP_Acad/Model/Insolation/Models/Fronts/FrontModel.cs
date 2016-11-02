using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Фронтоны - модель расчета
    /// </summary>
    public class FrontModel : ModelBase, IExtDataSave, ITypedDataValues
    {
        public FrontModel ()
        {
            Groups = new ObservableCollection<FrontGroup>();            
        }

        public InsModel Model { get; set; }

        public FrontOptions Options { get; set; }

        /// <summary>
        /// Группы - выбранные области на чертеже для расчета фронтонов
        /// </summary>
        public ObservableCollection<FrontGroup> Groups { get; set; } 

        public bool IsVisualsFrontOn { get { return isVisualsFrontOn; } set { isVisualsFrontOn = value; RaisePropertyChanged(); } }
        bool isVisualsFrontOn;


        /// <summary>
        /// Инициализация расчета
        /// </summary>        
        public void Initialize (InsModel insModel)
        {
            this.Model = insModel; 

            // Настройки
            if (Options == null)
            {
                Options = FrontOptions.Default();
            }
        }

        public void AddGroup (FrontGroup group)
        {            
            Groups.Add(group);
            // Обновлоение расчета группы
            group.Update();
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            throw new NotImplementedException();
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            throw new NotImplementedException();
        }

        public DicED GetExtDic (Document doc)
        {
            throw new NotImplementedException();
        }

        public void SetExtDic (DicED dicEd, Document doc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Подготовление к расчету фронтов
        /// </summary>
        public void InitToCalc ()
        {
            if (Options == null)            
                Options = FrontOptions.Default();            
            Options.DefineLayer(Model.Doc.Database);
        }
    }
}