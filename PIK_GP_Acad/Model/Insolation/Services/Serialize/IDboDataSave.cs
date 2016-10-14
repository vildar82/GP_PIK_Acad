using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Сохранение объекта в расширенные данные
    /// </summary>
    public interface IDboDataSave : IExtDataSave
    {        
        /// <summary>
        /// Объект автокада в который записываются расширенные данные
        /// </summary>
        /// <returns></returns>
        ObjectId GetDBObject ();        
    }
}
