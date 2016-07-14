using System;
using System.Collections.Generic;
using System.Linq;
using PIK_GP_Acad.FCS;

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

        // По классификации
        public double FC_LandArea { get; set; } // Площадь участа, га
        public double FC_QuarterArea { get; set; } // Площадь квартала, га
        public double FC_Density { get; set; } // Плотность м2/га

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

            // По классификатору
            if (_service.Classes.Count>0)
            {
                var landClass = _service.Classes.Find(c => c.ClassType.ClassName == "Участок");
                if (landClass != null)
                {
                    FC_LandArea = landClass.Value * 0.0001;
                    FC_QuarterArea = FC_LandArea;
                    foreach (var item in _service.Classes)
                    {
                        double val = GetreduceArea(item);
                        if (val != 0)
                        {
                            FC_QuarterArea -= val * 0.0001;
                        }
                    }
                    FC_Density = KP_GNS_Total / FC_QuarterArea;
                }
            }
        }

        private double GetreduceArea (IClassificator item)
        {
            double val = 0;
            switch (item.ClassType.ClassName)
            {
                case "УДС":
                case "ПК":
                case "Паркинг":
                case "Для вычитания":
                case "Участок СОШ":
                case "Участок ДОО":
                    val = item.Value;
                    break;
            }
            return val;
        }
    }
}