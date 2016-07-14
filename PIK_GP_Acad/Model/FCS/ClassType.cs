using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.FCS
{
    /// <summary>
    /// Тип классификатора
    /// </summary>
    public class ClassType
    {
        /// <summary>
        /// Имя классификатора (заданное в Civil)
        /// </summary>
        public string ClassName { get; set; }
        /// <summary>
        /// Имя строки записи в таблице
        /// </summary>
        public string TableName { get; set; }
        public ClassGroup Group { get; set; }
        public int Index { get; set; }
        public string Units { get; set; }
        public double UnitFactor { get; set; }

        public ClassType(string className, string tableName, ClassGroup group, int index,
            string units = "м" + AcadLib.General.Symbols.Square, double unitFactor =1 )
        {
            ClassName = className;
            TableName = tableName;
            Group = group;
            Index = index;
            Units = units;
            UnitFactor = unitFactor;
        }
    }
}
