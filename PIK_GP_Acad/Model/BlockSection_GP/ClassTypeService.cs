using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.FCS;

namespace PIK_GP_Acad.BlockSection_GP
{
    public class ClassTypeService : IClassTypeService
    {
        public const string Site = "Участок";
        static List<ClassType> classTypes = new List<ClassType>() {
             new ClassType(Site, "Площадь участка, га", null, 0),
             new ClassType("УДС", "", null, 1),
             new ClassType("ПК", "", null, 1),
             new ClassType("Паркинг", "", null, 1),
             new ClassType("Для вычитания", "", null, 1),
             new ClassType("Участок СОШ", "", null, 1),
             new ClassType("Участок ДОО", "", null, 1)
        };

        public ClassType GetClassType (string className)
        {
            var classType = classTypes.Find(c => c.ClassName.Equals(className, StringComparison.OrdinalIgnoreCase));
            return classType;
        }
    }
}
