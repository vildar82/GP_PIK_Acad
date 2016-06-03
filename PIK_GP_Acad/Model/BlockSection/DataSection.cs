using System.Collections.Generic;
using System.Linq;

namespace PIK_GP_Acad.BlockSection
{
    // Подсчет площедей и типоак блок-секций
    public class DataSection
    {
        private SectionService _service;       

        public double AverageFloors { get; private set; }
        public List<SectionType> SectionTypes { get; private set; }

        public double TotalArea { get; private set; }
        public double TotalAreaApart { get; private set; }

        public double TotalAreaBKFN { get; private set; }
        public double Population { get; set; }

        // КП
        public double KP_GNS_BKFN { get; set; }
        public double KP_GNS_Typical { get; set; }
        public double KP_GNS_Total { get; set; }

        public DataSection(SectionService sectionService)
        {
            _service = sectionService;
        }

        /// <summary>
        /// Подсчет площадей блок-секций и типов
        /// </summary>
        public void Calc()
        {
            // Разбивка секций на типы и подсчет общей площади счекций одного типа
            Dictionary<string, SectionType> types = new Dictionary<string, SectionType>();
            foreach (var section in _service.Sections)
            {
                SectionType secType;
                string key = section.Name.ToUpper();
                if (!types.TryGetValue(key, out secType))
                {
                    secType = new SectionType(section.Name, section.NumberFloor);
                    types.Add(key, secType);
                }
                secType.AddSection(section);

                KP_GNS_BKFN += section.AreaContour;
                KP_GNS_Typical += section.AreaContour * (section.NumberFloor - 1);
            }
            KP_GNS_Total = KP_GNS_BKFN + KP_GNS_Typical;
            SectionTypes = types.Values.ToList();
            SectionTypes.Sort();

            // Подсчет общих значений для всех типов секций
            var bsByFloor = SectionTypes.SelectMany(b => b.Sections).GroupBy(g => g.NumberFloor);
            var totalFloors = bsByFloor.Sum(s=>s.Key*s.Count());
            AverageFloors = totalFloors/(double)bsByFloor.Sum(s=>s.Count());
            TotalAreaApart = SectionTypes.Sum(s => s.AreaApartTotal);
            TotalAreaBKFN = SectionTypes.Sum(s => s.AreaBKFN);
            TotalArea = TotalAreaApart + TotalAreaBKFN;
        }
    }
}