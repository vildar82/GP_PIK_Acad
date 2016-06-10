using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;

namespace PIK_GP_Acad.ArcGIS
{
    /// <summary>
    /// Запуск программы ArcGIS
    /// </summary>
    public static class ArcGisService
    {
        public static void Start()
        {
            var sysDisk = Path.GetPathRoot(Environment.SystemDirectory);
            var arcGisDll = Path.Combine(sysDisk, @"Program Files\ArcGIS for AutoCAD 350\ArcGISForAutoCAD.dll");
            if (File.Exists(arcGisDll))
            {
                Assembly.LoadFrom(arcGisDll);
            }
            else
            {
                Inspector.AddError($"Не найдена программа ArcGIS - {arcGisDll}", System.Drawing.SystemIcons.Error);
            }
        }
    }
}
