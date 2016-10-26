using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AcadLib;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    public class TreeVisualOption : ModelBase, ITypedDataValues
    {
        public Color Color { get; set; }
        public int Height { get; set; }

        public TreeVisualOption () { }

        public TreeVisualOption(Color color, int height)
        {
            Color = color;
            Height = height;
        }        

        public static List<TreeVisualOption> DefaultTreeVisualOptions ()
        {
            List<TreeVisualOption> visuals = new List<TreeVisualOption> {
                new TreeVisualOption (Color.FromArgb(205, 32, 39), 35),
                new TreeVisualOption (Color.FromArgb(241, 235, 31), 55),
                new TreeVisualOption (Color.FromArgb(19, 155, 72), 75),
            };
            return visuals;
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            return new List<TypedValue> {
                TypedValueExt.GetTvExtData(Height),
                TypedValueExt.GetTvExtData(Color.A),
                TypedValueExt.GetTvExtData(Color.R),
                TypedValueExt.GetTvExtData(Color.G),
                TypedValueExt.GetTvExtData(Color.B),
            };
        }

        public static Color GetNextColor (Color color)
        {
            return ControlPaint.Dark(color);
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            if (values == null || values.Count != 5 )
            {
                // Дефолтные 
                // null
            }
            else
            {
                int index = 0;
                Height = values[index++].GetTvValue<int>();
                var a = values[index++].GetTvValue<byte>();
                var r = values[index++].GetTvValue<byte>();
                var g = values[index++].GetTvValue<byte>();
                var b = values[index++].GetTvValue<byte>();
                Color = Color.FromArgb(a, r, g, b);
            }
        }

        /// <summary>
        /// Проверка списка визуальных настроек елочек и корректировка при необходимости
        /// </summary>        
        public static void CheckAndCorrect (ref List<TreeVisualOption> treeVisOpts)
        {
            // Настройки высот должы быть:
            // Иметь высоту, цвет
            // Не повторять высоту
            // Отсортированы в порядке возрасчтания высоты

            // Проверка объектов по отдельности
            CheckItems(ref treeVisOpts);
            // Проверка повторяющихся высот
            CheckHeights(ref treeVisOpts);

            if (treeVisOpts.Count == 0)
            {
                treeVisOpts = DefaultTreeVisualOptions();
            }
        }

        private static void CheckHeights (ref List<TreeVisualOption> treeVisOpts)
        {
            var intComparer = new AcadLib.Comparers.IntEqualityComparer(1);
            // Сортировка по высоте
            treeVisOpts.Sort((t1, t2) => t1.Height.CompareTo(t2.Height));
            
            // Группировка по высоте с расхождением в 1м
            var groupsByH = treeVisOpts.GroupBy(g => g.Height, intComparer).Where(w=>w.Skip(1).Any()).ToList();            
            foreach (var groupH in groupsByH)
            {
                // Удалить лишнюю высоту
                foreach (var item in groupH.Skip(1))
                {
                    treeVisOpts.Remove(item);
                }
            }            
        }

        private static void CheckItems (ref List<TreeVisualOption> treeVisOpts)
        {
            var incorectItems = new List<TreeVisualOption>();
            foreach (var item in treeVisOpts)
            {
                if (!item.IsCorrect())
                {
                    incorectItems.Add(item);
                }
            }

            foreach (var item in incorectItems)
            {
                treeVisOpts.Remove(item);
            }
        }

        private bool IsCorrect ()
        {
            if (Height == 0) return false;
            if (Color == null) return false;
            return true;
        }
    }
}
