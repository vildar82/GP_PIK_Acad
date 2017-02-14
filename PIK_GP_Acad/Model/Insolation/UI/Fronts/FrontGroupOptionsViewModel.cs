using Autodesk.AutoCAD.DatabaseServices;
using MicroMvvm;
using PIK_GP_Acad.Insolation.Models;
using PIK_GP_Acad.Insolation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PIK_GP_Acad.Insolation.UI
{
    /// <summary>
    /// Настройки группы фронтов
    /// </summary>
    public class FrontGroupOptionsViewModel : ViewModelBase
    {        
        public FrontGroupOptionsViewModel(FrontGroupOptions options, bool hasSelectExtents, int frontLevel)
        {
            HasSelectButton = hasSelectExtents;
            FrontLevel = frontLevel;
            Options = options;
            WindowVM = new WindowOptionsViewModel(Options.Window);
            OK = new RelayCommand(OnOkExecute);
            SelectExtents = new RelayCommand(OnSelectExtentsExecute);
        }        
        
        public FrontGroupOptions Options { get; set; }

        public WindowOptionsViewModel WindowVM { get; set; }
        public int FrontLevel { get { return frontLevel; } set { frontLevel = value; RaisePropertyChanged(); } }
        private int frontLevel;

        public bool? DialogResult { get { return dlgres; } set { dlgres = value; RaisePropertyChanged(); } }
        bool? dlgres;

        public bool HasSelectButton { get; set; }
        public Extents3d? SelectedExtents { get; set; }

        public RelayCommand OK { get; set; }
        public RelayCommand SelectExtents { get; set; }

        private void OnOkExecute()
        {
            DialogResult = true;
            Options.Window = WindowVM.Window;      
        }

        private void OnSelectExtentsExecute()
        {
            var selectGroup = new SelectGroup(Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument);
            var ext = selectGroup.Select();
            SelectedExtents = ext;
        }
    }    
}
namespace ExCastle.Wpf
{
    public static class DialogCloser
    {
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(DialogCloser),
                new PropertyMetadata(DialogResultChanged));

        private static void DialogResultChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window != null)
                window.DialogResult = e.NewValue as bool?;
        }
        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
    }
}
