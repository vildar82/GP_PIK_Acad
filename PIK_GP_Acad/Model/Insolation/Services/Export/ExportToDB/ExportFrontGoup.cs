using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services.Export
{
    /// <summary>
    /// Экспорт группы домов - фронтов инсоляции
    /// </summary>
    public class ExportFrontGoup
    {
        private FrontGroup group;

        public ExportFrontGoup(FrontGroup item)
        {
            this.group = item;
        }

        /// <summary>
        /// Получение экспортных данных инсоляции фронтов группы домов (одного расчета Жуков)
        /// </summary>        
        public ExportInsData GetExportInsData ()
        {
            // Преобразование группы - перенос в 0,0 (как будет в Excel)
            Transformation();

            return null;
        }

        /// <summary>
        /// Преобразование группы - перенос в 0,0 (как будет в Excel)
        /// </summary>
        private void Transformation()
        {
            // Преобразование домов
            var housesTrans = new List<HouseTransform>();
            foreach (var item in group.Houses)
            {
                housesTrans.Add(new HouseTransform(item));
            }
        }
    }
}
