using System;
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
        public ExportGroupsViewModel (List<FrontGroup> exportedGroups, List<FrontGroup> notIdentifiedGroups)
        {
            ExportedGroups = new ObservableCollection<FrontGroup> ( exportedGroups);
            NotIdentifiedGroups = new ObservableCollection<FrontGroup> (notIdentifiedGroups);
            OK = new RelayCommand(OnOkExecute);
        }        

        public RelayCommand OK { get; set; }

        public ObservableCollection<FrontGroup> ExportedGroups { get; set; }
        public ObservableCollection<FrontGroup> NotIdentifiedGroups { get; set; }

        private void OnOkExecute()
        {
                        
        }
    }

    public class ExportGroupsViewModelTest : ExportGroupsViewModel
    {
        public ExportGroupsViewModelTest()
        {
            ExportedGroups = FillGroups( "Блок", 3, "Корпус", 3);
            NotIdentifiedGroups = FillGroups( "Группа", 2, "Дом", 4);
        }

        private ObservableCollection<FrontGroup> FillGroups (string groupPrefix, int groupsCount, string housePrefix, int houseCount)
        {
            var groups = new ObservableCollection<FrontGroup>();
            for (int i = 0; i < groupsCount; i++)
            {
                var group = new FrontGroup();
                group.Name = groupPrefix + i;
                group.Houses = new ObservableCollection<House>();
                for (int h = 0; h < houseCount; h++)
                {
                    var house = new House();
                    house.Name = housePrefix + h;
                    group.Houses.Add(house);
                }
                groups.Add(group);
            }
            return groups;
        }
    }
}
