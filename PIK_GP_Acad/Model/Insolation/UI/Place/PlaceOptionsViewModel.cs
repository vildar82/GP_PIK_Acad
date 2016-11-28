using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MicroMvvm;
using PIK_GP_Acad.Insolation.Models;
using PIK_GP_Acad.Insolation.Services;
using System.Collections.ObjectModel;

namespace PIK_GP_Acad.Insolation.UI
{
    public class PlaceOptionsViewModel : ViewModelBase
    {
        public PlaceOptionsViewModel()
        {
        }

        public PlaceOptionsViewModel(PlaceOptions placeOptions)
        {
            PlaceOptions = placeOptions;
            TileSize = placeOptions.TileSize;
            TransparenceInvert = (byte)(255 - placeOptions.Transparent);
            Levels = new ObservableCollection<TileLevel>(placeOptions.Levels);

            SelectColor = new RelayCommand<TileLevel>(OnSelectColorExecute);
            DeleteLevel = new RelayCommand<TileLevel>(OnDeleteLevelExecute, OnDeleteLevelCanExecute);
            AddLevel = new RelayCommand(OnAddLevelExecute, OnAddLevelCanExecute);
            OK = new RelayCommand(OnOkExecute);
            ResetLevels = new RelayCommand(OnResetLevelsExecute);

            foreach (var item in Levels)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }        

        /// <summary>
        /// Модель
        /// </summary>
        public PlaceOptions PlaceOptions { get; set; }

        public ObservableCollection<TileLevel> Levels { get; set; }

        public double TileSize { get { return tileSize; } set { tileSize = value; RaisePropertyChanged(); } }
        double tileSize;
        public byte TransparenceInvert { get { return transparenceInvert; } set { transparenceInvert = value; RaisePropertyChanged(); } }
        byte transparenceInvert;

        public RelayCommand<TileLevel> SelectColor { get; set; }
        public RelayCommand<TileLevel> DeleteLevel { get; set; }
        public RelayCommand AddLevel { get; set; }
        public RelayCommand OK { get; set; }
        public RelayCommand ResetLevels { get; set; }

        private void OnSelectColorExecute (TileLevel level)
        {
            level.Color = InsService.ColorPicker(level.Color);
        }

        private void OnDeleteLevelExecute (TileLevel level)
        {
            Levels.Remove(level);
        }

        private bool OnDeleteLevelCanExecute (TileLevel level)
        {
            return Levels.Count > 1;
        }

        private void OnResetLevelsExecute()
        {
            Levels.Clear();
            var defLevels = TileLevel.Defaults();
            foreach (var item in defLevels)
            {
                Levels.Add(item);
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        private void OnAddLevelExecute ()
        {
            var level = new TileLevel();
            var lastLevel = Levels.Last();            
            level.Color = ControlPaint.Dark(lastLevel.Color);
            Levels.Add(level);
            level.PropertyChanged += Item_PropertyChanged;
            level.TotalTimeH = lastLevel.TotalTimeH + 1;            
        }
        private bool OnAddLevelCanExecute ()
        {
            return Levels.Count < 4;
        }

        private void Item_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TileLevel.TotalTimeH))
            {
                LevelTimeChanged();
            }
        }

        private void LevelTimeChanged ()
        {
            // Провеерка высот
            var levels = Levels.ToList();
            levels = TileLevel.CheckAndCorrect(levels);
            Levels.Clear();
            foreach (var item in levels)
            {
                Levels.Add(item);
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        private void OnOkExecute()
        {
            PlaceOptions.Levels = Levels;
            PlaceOptions.TileSize = TileSize;
            PlaceOptions.Transparent = (byte)(255 -TransparenceInvert);
        }
    }
}
