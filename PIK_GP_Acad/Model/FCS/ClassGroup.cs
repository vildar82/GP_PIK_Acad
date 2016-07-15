using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.FCS
{
    public class ClassGroup : IEquatable<ClassGroup>
    {
        public string Name { get; set; }
        public string TotalName { get; set; }

        public ClassGroup(string name, string totalName)
        {
            Name = name;
            TotalName = totalName;        
        }

        public bool Equals (ClassGroup other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            var res = Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
            return res;
        }

        public override int GetHashCode ()
        {
            return Name.GetHashCode();
        }
    }
}
