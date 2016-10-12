using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Сохранение объекта в словарь NOD
    /// </summary>
    public interface INodDataSave
    {        
        /// <summary>
        /// Список сохраняемяхъ значений
        /// </summary>
        /// <returns></returns>
        List<TypedValue> GetDataValues (Document doc);
        void SetDataValues (List<TypedValue> values, Document doc);
    }
}
