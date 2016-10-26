using System;
using System.Linq;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;
using System.Collections.ObjectModel;
using System.Drawing;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.UI
{
    public class TreeOptionsViewModel : ViewModelBase
    {        
        public TreeOptionsViewModel (TreeOptions treeOptions)
        {
            TreeOptionsModel = treeOptions;
            TreeVisualOptions = treeOptions.TreeVisualOptions;

            AddVisualTree = new RelayCommand (OnAddVisualTreeExecute, OnAddVisualTreeCanExecute);
            DeleteVisualTree = new RelayCommand<TreeVisualOption>(OnDeleteVisualTreeExecute, OnDeleteVisualTreeCanExecute);
            SelectColor = new RelayCommand(OnSelectColorExecute);
        }
        
        TreeOptions TreeOptionsModel { get; set; }

        public ObservableCollection<TreeVisualOption> TreeVisualOptions { get; set; }

        public TreeVisualOption SelectedVisualTree { get; set; }

        public RelayCommand AddVisualTree { get; set; }
        public RelayCommand SelectColor { get; set; }
        public RelayCommand<TreeVisualOption> DeleteVisualTree { get; set; }

        //protected override Task InitializeAsync ()
        //{
        //    foreach (var item in TreeVisualOptions)
        //    {
        //        item.PropertyChanged += VisualTree_PropertyChanged;
        //    }
        //    return base.InitializeAsync();
        //}

        //protected override Task<bool> SaveAsync ()
        //{
        //    //TreeOptionsModel.TreeVisualOptions = TreeVisualOptions;
        //    return base.SaveAsync();
        //}

        //protected override Task CloseAsync ()
        //{
        //    foreach (var item in TreeVisualOptions)
        //    {
        //        item.PropertyChanged -= VisualTree_PropertyChanged;
        //    }
        //    return base.CloseAsync();
        //}

        private void VisualTree_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TreeVisualOption.Height))
            {
                HeightChanged();
            }
        }

        private void HeightChanged()
        {
            // Провеерка высот
            var treeVisOpts = TreeVisualOptions.ToList();
            TreeVisualOption.CheckAndCorrect(ref treeVisOpts);
            TreeVisualOptions.Clear();
            foreach (var item in treeVisOpts)
            {
                TreeVisualOptions.Add(item);
                //item.PropertyChanged += VisualTree_PropertyChanged;
            }
        }

        private void OnAddVisualTreeExecute ()
        {
            var lastVisOpt = TreeVisualOptions.Last();
            var c = TreeVisualOption.GetNextColor(lastVisOpt.Color);
            var visTree = new TreeVisualOption(c, lastVisOpt.Height+10);
            TreeVisualOptions.Add(visTree);
            //visTree.PropertyChanged += VisualTree_PropertyChanged;
        }

        private bool OnAddVisualTreeCanExecute ()
        {
            return TreeVisualOptions.Count < 4;
        }

        private bool OnDeleteVisualTreeCanExecute (TreeVisualOption arg)
        {
            return TreeVisualOptions.Count > 1;
        }

        private void OnDeleteVisualTreeExecute (TreeVisualOption arg)
        {
            TreeVisualOptions.Remove(arg);
        }

        private void OnSelectColorExecute ()
        {
            SelectedVisualTree.Color = ColorPicker(SelectedVisualTree.Color);
        }

        private Color ColorPicker (Color current)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.AnyColor = true;
            colorDialog.FullOpen = true;           
            colorDialog.Color = current;
            colorDialog.AllowFullOpen = true;
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return colorDialog.Color;
            }
            return current;
        }
    }

    public class DesignTreeOptionsViewModel
    {
        public ObservableCollection<TreeVisualOption> TreeVisualOptions { get; set; }

        public DesignTreeOptionsViewModel()
        {
            TreeVisualOptions = new ObservableCollection<TreeVisualOption>(TreeVisualOption.DefaultTreeVisualOptions());
        }
    }
}
