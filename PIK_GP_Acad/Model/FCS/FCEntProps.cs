using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using NetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.FCS
{
    /// <summary>
    /// Свойства в классификаторе элемента
    /// </summary>
    public class FCEntProps
    {
        private Dictionary<string, FCProperty> dictProps;
        private List<FCProperty> props;
        public FCEntProps(string classname, ObjectId idEnt, IEnumerable<FCProperty> props)
        {
            Class = classname;
            IdEnt = idEnt;
            this.dictProps = props.ToDictionary(k => k.Name, v => v);
        }

        public string Class { get; set; }
        public ObjectId IdEnt { get; set; }

        public FCProperty GetProperty(string name)
        {
            FCProperty prop;
            dictProps.TryGetValue(name, out prop);
            return prop;
        }

        public T GetPropertyValue <T>(string name, T defaultValue, bool isRequired = false)
        {
            var prop = GetProperty(name);
            if (prop == null)
            {
                return defaultValue;
            }
            try
            {
                return prop.Value.GetValue<T>();
            }
            catch (Exception ex)
            {
                if (isRequired)
                {
                    Inspector.AddError($"Не определен параметр '{name}' класифицированного элемента. {ex.Message}" , IdEnt, System.Drawing.SystemIcons.Error);
                }
            }
            return defaultValue;
        }

        public List<FCProperty> GetProperties()
        {
            return props;
        }
    }
}
