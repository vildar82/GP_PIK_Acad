using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Настройки площадок
    /// </summary>
    public class PlaceOptions : ModelBase, IExtDataSave, ITypedDataValues
    {
        public PlaceOptions()
        {

        }

        /// <summary>
        /// Размер ячейки площадки (шаг расчетных точек на площадке) [м]
        /// </summary>
        public double TileSize { get { return tileSize; } set { tileSize = value; RaisePropertyChanged(); } }
        double tileSize;

        public ObservableCollection<TileLevel> Levels { get { return levels; } set { levels = value; RaisePropertyChanged(); } }
        ObservableCollection<TileLevel> levels;

        public byte Transparent { get; set; } = 60;

        public static PlaceOptions Default ()
        {
            var opt = new PlaceOptions();
            opt.TileSize = 1;
            opt.Levels = TileLevel.Defaults();
            return opt;
        }

        public DicED GetExtDic (Document doc)
        {
            var dicOpt = new DicED();
            dicOpt.AddRec("Recs", GetDataValues(doc));
            var dicLevels = new DicED("Levels");
            for (int i = 0; i < Levels.Count; i++)
            {
                var l = Levels[i];
                dicLevels.AddRec("Level" + i, l.GetDataValues(doc));
            }
            return dicOpt;
        }

        public void SetExtDic (DicED dicOpt, Document doc)
        {
            SetDataValues(dicOpt?.GetRec("Recs")?.Values, doc);
            var dicLevels = dicOpt?.GetInner("Levels");
            int index = 0;
            RecXD recL;
            do
            {
                recL = dicLevels?.GetRec("Level" + index++);
                if (recL != null)
                {
                    var level = new TileLevel();
                    level.SetDataValues(recL.Values, doc);
                    Levels.Add(level);
                }
            } while (recL != null && index<4);
            if (Levels == null)
            {
                // Дефолтные уровни
                Levels = TileLevel.Defaults();
            }
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            var tvk = new TypedValueExtKit();
            tvk.Add("TileSize", TileSize);
            tvk.Add("Transparent", Transparent);
            return tvk.Values;            
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            var dictValues = values?.ToDictionary();
            TileSize = dictValues.GetValue("TileSize", 1d);
            Transparent = dictValues.GetValue("Transparent", (byte)60);            
        }
    }
}
