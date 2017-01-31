﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.Services.Export
{
    /// <summary>
    /// Модель отображения для экпортируемых и неэкспортируемых групп (блоков)
    /// </summary>
    public class ExportGroupsViewModel : ViewModelBase
    {        
        public ExportGroupsViewModel()
        {

        }

        public ExportGroupsViewModel (List<FrontGroup> exportedGroups)
        {
            ExportGroups = new ObservableCollection<GroupViewModel>();
            foreach (var item in exportedGroups)
            {
                ExportGroups.Add(new GroupViewModel(item));
            }
            //NotIdentifiedGroups = new ObservableCollection<FrontGroup> (notIdentifiedGroups);
            Export = new RelayCommand(OnExportExecute, () => exportedGroups.Any(g => g.Houses.Any(h => h.SelectedHouseDb != null)));
        }        

        public RelayCommand Export { get; set; }

        public ObservableCollection<GroupViewModel> ExportGroups { get; set; }
        //public ObservableCollection<FrontGroup> NotIdentifiedGroups { get; set; }

        private void OnExportExecute()
        {
            // Пока ничего. Потом, возможно, добавлю список выбранных групп для экспорта                        
        }        
    }    
}
