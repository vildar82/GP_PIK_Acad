using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Elements;
using PIK_GP_Acad.OD;

namespace PIK_GP_Acad.Elements.InfraworksExport
{
    /// <summary>
    /// Объект экспортируемый в инфраворкс
    /// </summary>
    public interface IInfraworksExport : IElement
    {
        /// <summary>
        /// Записи OD
        /// </summary>        
        List<IODRecord> GetODRecords ();
    }
}
