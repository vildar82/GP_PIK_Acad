using NetLib;
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

    public static class FCPropertyExt
    {
        public static T GetPropValue<T>(this IEnumerable<FCProperty> fcs, string propName, T defaultValue)
        {            
            var prop = fcs?.FirstOrDefault(p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));
            if (prop != null)
            {
                try
                {
                    var res = prop.Value.GetValue<T>();
                    if (EqualityComparer<T>.Default.Equals(res, default(T)))
                    {
                        return defaultValue;
                    }
                    return res;
                }
                catch { }
            }            
            return defaultValue;
        }
    }
}
