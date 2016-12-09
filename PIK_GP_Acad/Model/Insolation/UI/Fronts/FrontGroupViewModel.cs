using MicroMvvm;
using PIK_DB_Projects;
using PIK_GP_Acad.Insolation.Models;
using PIK_GP_Acad.Insolation.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.UI
{
    /// <summary>
    /// Вью-модель для окна группы (добавления/редактирования)
    /// </summary>
    public class FrontGroupViewModel : ViewModelBase
    {
        public FrontGroupViewModel()
        {

        }

        public FrontGroupViewModel(FrontGroup frontGroup)
        {
            FrontGroup = frontGroup;
            Name = frontGroup.Name;
            ObjectsMDM = GetObjectsMDM();            
        }        

        /// <summary>
        /// Model
        /// </summary>
        public FrontGroup FrontGroup { get; set; }

        public string Name { get { return name; } set { name = value; RaisePropertyChanged(); } }
        string name;

        /// <summary>
        /// Список домов в группе - и список свободных корпусов
        /// </summary>
        public List<KeyValuePair<House, ObservableCollection<ObjectMDM>>> ObjectsMDM { get; set; }

        /// <summary>
        /// Заполнение списка блоков из базы
        /// </summary>
        private List<KeyValuePair<House, ObservableCollection<ObjectMDM>>> GetObjectsMDM()
        {
            var objsMdm = DbService.GetHouses(FrontGroup.Front.Model.Options.Project);
            if (objsMdm == null || objsMdm.Count == 0) return null;
            var observColObj = new ObservableCollection<ObjectMDM>(objsMdm);
            observColObj.Add(null);
            var resCol = FrontGroup.Houses.Select(s => new KeyValuePair<House, ObservableCollection<ObjectMDM>>(s, observColObj)).ToList();
            return resCol;
        }
    }
}
