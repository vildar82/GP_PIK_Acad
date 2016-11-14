using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MicroMvvm;
using PIK_GP_Acad.Insolation.Models;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.UI
{
    public class PlaceOptionsViewModel : ViewModelBase
    {
        public PlaceOptionsViewModel(PlaceOptions placeOptions)
        {
            PlaceOptions = placeOptions;

            SelectColor = new RelayCommand<TileLevel>(OnSelectColorExecute);
            DeleteLevel = new RelayCommand<TileLevel>(OnDeleteLevelExecute, OnDeleteLevelCanExecute);
            AddLevel = new RelayCommand(OnAddLevelExecute, OnAddLevelCanExecute);

            foreach (var item in placeOptions.Levels)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }        

        /// <summary>
        /// Модель
        /// </summary>
        public PlaceOptions PlaceOptions { get; set; }

        public RelayCommand<TileLevel> SelectColor { get; set; }
        public RelayCommand<TileLevel> DeleteLevel { get; set; }
        public RelayCommand AddLevel { get; set; }

        private void OnSelectColorExecute (TileLevel level)
        {
            level.Color = InsService.ColorPicker(level.Color);
        }

        private void OnDeleteLevelExecute (TileLevel level)
        {
            PlaceOptions.Levels.Remove(level);
        }

        private bool OnDeleteLevelCanExecute (TileLevel level)
        {
            return PlaceOptions.Levels.Count > 1;
        }

        private void OnAddLevelExecute ()
        {
            var level = new TileLevel();
            var lastLevel = PlaceOptions.Levels.Last();            
            level.Color = ControlPaint.Dark(lastLevel.Color);
            PlaceOptions.Levels.Add(level);
            level.PropertyChanged += Item_PropertyChanged;
            level.TotalTimeH = lastLevel.TotalTimeH + 1;            
        }
        private bool OnAddLevelCanExecute ()
        {
            return PlaceOptions.Levels.Count < 4;
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
            var levels = PlaceOptions.Levels.ToList();
            levels = TileLevel.CheckAndCorrect(levels);
            PlaceOptions.Levels.Clear();
            foreach (var item in levels)
            {
                PlaceOptions.Levels.Add(item);
                item.PropertyChanged += Item_PropertyChanged;
            }
        }
    }
}
