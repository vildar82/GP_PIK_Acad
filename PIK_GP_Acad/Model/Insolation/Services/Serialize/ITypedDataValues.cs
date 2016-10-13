using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Services
{
    public interface ITypedDataValues
    {        
        /// <summary>
        /// Список сохраняемяхъ значений
        /// </summary>
        /// <returns></returns>
        List<TypedValue> GetDataValues (Document doc);
        /// <summary>
        /// Установка значений
        /// </summary>
        /// <param name="values"></param>
        void SetDataValues (List<TypedValue> values, Document doc);
    }
}