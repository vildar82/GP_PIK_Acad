using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Elements;
using PIK_GP_Acad.FCS;

namespace PIK_GP_Acad.FCS.Balance
{
    class BalanceRow : IFCRow
    {
        List<IArea> items;       

        public ClassType ClassType { get; set; }
        public string Name { get; set; }
        public string Units { get; set; }
        public double Area { get; set; }
        public double PercentTerritory { get; set; }

        public BalanceRow (List<IArea> items)
        {
            this.items = items;
            var first = items.First();
            ClassType = first.ClassType;
            Name = first.ClassType.TableName;
            Units = first.ClassType.Units;
            Area = items.Sum(i => i.Area);
        }
    }
}
