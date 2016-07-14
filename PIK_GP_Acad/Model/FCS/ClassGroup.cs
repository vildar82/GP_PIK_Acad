using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.FCS
{
    public class ClassGroup
    {
        public static readonly ClassGroup HardCoating = new ClassGroup("Твердые покрытия", "Всего площадь твердых покрытий:");        

        public string Name { get; set; }
        public string TotalName { get; set; }

        private ClassGroup(string name, string totalName)
        {
            Name = name;
            TotalName = totalName;        
        }
    }
}
