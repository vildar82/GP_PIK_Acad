using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.FCS
{
    public class FCProperty
    {        
        public string Name { get; }        
        public object Value { get; set; }

        public FCProperty(string name, object value)
        {            
            Name = name;            
            Value = value;
        }        
    }
}
