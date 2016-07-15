using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.FCS;

namespace PIK_GP_Acad.FCS.Balance
{
    public class BalanceClassService : IClassTypeService
    {
        public static readonly ClassGroup HardCoating = new ClassGroup("Твердые покрытия", "Всего площадь твердых покрытий:");
        public static readonly ClassGroup Landscaping = new ClassGroup("Озеленение и благоустройство", "Всего площадь озеленения и благоустройства:");

        public const string HomeArea = "Участок дома";
        static List<ClassType> classTypes = new List<ClassType>() {
             new ClassType(HomeArea, "Площадь участка дома", null, 0),
             new ClassType("Размещаемые БРП_ТП", "Площадь размещаемой БРП, ТП", null, 1),
             new ClassType("Жилые здания", "Площадь застройки дома", null, 2),
             new ClassType("Общественные здания", "Площадь застройки дома", null, 2),
             new ClassType("Асфальтобетон", "Площадь проектируемых проездов и автостоянок с покрытием асфальтобетоном", HardCoating, 3),
             new ClassType("Тротуарная плитка_Тротуары", "Прощадь проектируемых троутаров с покрытием тротуарной плиткой", HardCoating, 4),
             new ClassType("Тротуарная плитка_Тротуары с проездом", "Площадь проектируемых усиленных тротуаров с покрытием тротуарной плиткой с возможностью проезда пожарного автотранспорта", HardCoating, 5),
             new ClassType("Газон", "Площадь газонов", Landscaping, 6),
             new ClassType("Газон_отмостка", "Площадь отмостки с газонным покрытием", Landscaping, 7),
             new ClassType("Гравийные высевки_ДП", "Детские игровые площадки с покрытием гравийными высевками", Landscaping, 8),
             new ClassType("Гравийные высевки_ПО", "Площадки отдыха взрослого населения с покрытием гравийными высевками", Landscaping, 9),
             new ClassType("Гравийные высевки_ПАО", "Площадка активного отдыха гравийными высевками", Landscaping, 10),
             new ClassType("Газонная решетка_Проезд пож.транспорта", "Площадь проездов пожарного автотранспорта с покрытием газонной решеткой", Landscaping, 11)
        };

        public ClassType GetClassType (string tag)
        {
            var classType = classTypes.Find(c => c.ClassName.Equals(tag, StringComparison.OrdinalIgnoreCase));            
            return classType;            
        }
    }
}
