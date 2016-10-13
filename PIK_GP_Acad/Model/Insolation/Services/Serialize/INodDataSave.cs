using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.XData;
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
        /// Словарь для сохранения объекта
        /// </summary>        
        DicED GetExtDic (Document doc);
        /// <summary>
        /// установить значения из словаря в объект
        /// </summary>        
        void SetExtDic (DicED DicED, Document doc);
    }
}
