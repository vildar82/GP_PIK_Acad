using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Catel.Data;
using PIK_GP_Acad.Insolation.Services;
using AcadLib;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Настройки елочек
    /// </summary>
    public class TreeOptions : ModelBase, INodDataSave
    {
        /// <summary>
        /// Список высотностей для елочек
        /// Должны быть отсортированы по возрастанию высоты и не должно быть повторений высот
        /// </summary>
        public ObservableCollection<TreeVisualOption> TreeVisualOptions { get; set; }

        public TreeOptions Default ()
        {
            TreeOptions defTreeOpt = new TreeOptions();
            defTreeOpt.TreeVisualOptions = TreeVisualOption.DefaultTreeVisualOptions();
            return defTreeOpt;
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            List<TypedValue> values = new List<TypedValue>();
            foreach (var item in TreeVisualOptions)
            {
                 item.GetDataValues(doc);
            }
            return values;
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {            
            throw new NotImplementedException();
        }
    }
}
