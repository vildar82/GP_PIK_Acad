using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Central
{
    /// <summary>
    /// Расчет Елочек в центральном регионе
    /// </summary>
    public class TreesCentral : IInsTreeService
    {
        private CentralInsService insService;

        public TreesCentral (CentralInsService centralInsService)
        {
            insService = centralInsService;
        }

        /// <summary>
        /// Добавление расчетной точки
        /// </summary>        
        public IInsPoint AddPoint ()
        {
            // Запрос точки у пользователя.
            // Проверка правильности указанной точки - должен быть определен дом для этой точки
            // Запрос параметров точки
            //throw new NotImplementedException();
            // Test
            return new InsPoint();
        }
    }
}
