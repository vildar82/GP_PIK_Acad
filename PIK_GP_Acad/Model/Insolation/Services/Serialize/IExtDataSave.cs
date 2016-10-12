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
    public interface IExtDataSave
    {
        /// <summary>
        /// Имя записи - InsPoint
        /// </summary>
        string DataRecName { get;  }
        /// <summary>
        /// Объект автокада в который записываются расширенные данные
        /// </summary>
        /// <returns></returns>
        ObjectId GetDBObject ();
        /// <summary>
        /// Список сохраняемяхъ значений
        /// </summary>
        /// <returns></returns>
        List<TypedValue> GetDataValues ();
    }
}
