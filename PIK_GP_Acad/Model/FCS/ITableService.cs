using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Tables;
using PIK_GP_Acad.Elements;

namespace PIK_GP_Acad.FCS
{
    /// <summary>
    /// Таблица ТЕПов
    /// </summary>
    public interface ITableService : ICreateTable
    {   
        /// <summary>
        /// Ссписок объектов сгруппированных по имени в таблице
        /// </summary>        
        void CalcRows (List<IGrouping<string, IArea>> groups);
    }
}
