using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.Models;
using MicroMvvm;
using AcadLib.Errors;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsViewModel : ViewModelBase
    {    
        public InsViewModel ()
        {            
            EditInsOptions = new RelayCommand(OnEditInsOptionsExecute);
            Update = new RelayCommand(OnUpdateExecute);
        }

        InsModel model;
        public InsModel Model {
            get { return model; }
            set {
                model = value;
                RaisePropertyChanged();
                UpdateBinding();
            }
        }

        public RelayCommand EditInsOptions { get; private set; }
        public RelayCommand Update { get; private set; }        

        public TreesViewModel TreeVM { get { return treeVM; } set { treeVM = value; RaisePropertyChanged(); } }
        TreesViewModel treeVM;

        public FrontViewModel FrontVM { get { return frontVM; } set { frontVM = value; RaisePropertyChanged(); } }
        FrontViewModel frontVM;

        public PlaceViewModel PlaceVM { get { return placeVM; } set { placeVM = value; RaisePropertyChanged(); } }
        PlaceViewModel placeVM;        

        public string City {
            get { return Model?.Options.Region.City; }
            set { RaisePropertyChanged(); }
        }
        public double Latitude {
            get { return Model?.Options.Region.Latitude ?? 0; }
            set { RaisePropertyChanged(); }
        }

        private void OnEditInsOptionsExecute()
        {
            var optVM = new InsOptionsViewModel(Model.Options);
            if (InsService.ShowDialog(optVM) == true)
            {
                bool needUpdate;                
                Model.SetOptions(optVM.InsOptions, out needUpdate);
                if (needUpdate)
                    OnUpdateExecute();
                UpdateBinding();
            }
        }

        private void OnUpdateExecute ()
        {
            try
            {
                Inspector.Clear();
                Model?.Update();
            }
            catch(Exception ex)
            {
                InsService.ShowMessage(ex, "");
            }
            Inspector.Show();
            Inspector.Clear();
        }

        private void UpdateBinding ()
        {
            if (Model != null)
            {
                City = Model.Options.Region.City;
                Latitude = Model.Options.Region.Latitude;
                TreeVM = new TreesViewModel(Model.Tree);
                FrontVM = new FrontViewModel(Model.Front);
                PlaceVM = new PlaceViewModel(Model.Place);
            }
        }        
    }
}
