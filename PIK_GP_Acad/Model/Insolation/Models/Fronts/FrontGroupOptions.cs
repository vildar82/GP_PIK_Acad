using AcadLib.XData;
using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Настройка группы фронтов
    /// </summary>
    public class FrontGroupOptions : ModelBase, IExtDataSave, ITypedDataValues
    {
        public FrontGroupOptions()
        {

        }

        /// <summary>
        /// Вид окон для расчета фронтов
        /// </summary>
        public WindowOptions Window { get; set; } = WindowOptions.Default();        

        public DicED GetExtDic(Document doc)
        {
            var dicOpt = new DicED();
            dicOpt.AddInner("Window", Window?.GetExtDic(doc));
            dicOpt.AddRec("FrontGroupOptionsRec", GetDataValues(doc));            
            return dicOpt;
        }
        public void SetExtDic(DicED dicOpt, Document doc)
        {
            SetDataValues(dicOpt?.GetRec("FrontGroupOptionsRec")?.Values, doc);
            Window = new WindowOptions();
            Window.SetExtDic(dicOpt?.GetInner("Window"), doc);
        }
        
        public List<TypedValue> GetDataValues(Document doc)
        {
            return null;
        }
        public void SetDataValues(List<TypedValue> values, Document doc)
        {            
        }
    }
}
