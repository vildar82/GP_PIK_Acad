﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.Models;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsViewModel : ViewModelBase
    {    
        public InsViewModel ()
        {            
            SelectRegion = new RelayCommand(OnSelectRegionExecute);
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

        public RelayCommand SelectRegion { get; private set; }
        public RelayCommand Update { get; private set; }

        public TreesViewModel TreeVM { get { return treeVM; } set { treeVM = value; RaisePropertyChanged(); } }
        TreesViewModel treeVM;

        public string UpdateInfo {
            get { return Model?.UpdateInfo; }
            set { Model.UpdateInfo = value;
                RaisePropertyChanged();
            }
        }

        public bool IsUpdateRequired {
            get { return Model?.IsUpdateRequired ?? false; }
            set { Model.IsUpdateRequired = value;
                RaisePropertyChanged();
            }
        }

        public string City {
            get { return Model?.Options.Region.City; }
            set { RaisePropertyChanged(); }
        }
        public double Latitude {
            get { return Model?.Options.Region.Latitude ?? 0; }
            set { RaisePropertyChanged(); }
        }

        private void OnSelectRegionExecute ()
        {            
            var regVM = new InsRegionViewModel(Model.Options.Region);                        
            if (InsService.ShowDialog(regVM) == true)
            {
                Model.Options.Region = regVM.InsRegion;
                Model.Update();
                UpdateBinding();                
            }            
        }

        private void OnUpdateExecute ()
        {
            Model?.Update();
        }

        private void UpdateBinding ()
        {
            if (Model != null)
            {
                City = Model.Options.Region.City;
                Latitude = Model.Options.Region.Latitude;
                TreeVM = new TreesViewModel(Model.Tree);
            }
        }        
    }
}
