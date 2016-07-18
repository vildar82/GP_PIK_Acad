using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.OD
{
    /// <summary>
    /// Параметр Object Data
    /// </summary>
    public class ODParameter
    {
        /// <summary>
        /// Тип параметра (стока, число)
        /// </summary>
        public DataType Type { get; set; }
        /// <summary>
        /// Имя параметра
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Значение
        /// </summary>
        public dynamic Value { get; set; }
        public dynamic DefaultValue { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Индекс параметра в таблице OD
        /// </summary>
        public int Index { get; set; }

        public ODParameter (string name, DataType type, string desc, dynamic defaultValue)
        {
            Name = name;
            Type = type;
            Description = desc;
            DefaultValue = defaultValue;
        }

        public string GetInfo ()
        {
            return $"'{Name}'-{Type}";
        }
    }
}
