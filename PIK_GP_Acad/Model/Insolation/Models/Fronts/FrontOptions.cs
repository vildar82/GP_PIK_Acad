using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Насройки расчета фронотов
    /// </summary>
    public class FrontOptions : ModelBase, IExtDataSave, ITypedDataValues
    {
        public FrontOptions()
        {

        }

        /// <summary>
        /// Шаг расчетной точки по фронту
        /// </summary>
        public double StepCalcPointInFront { get { return stepCalcPointInFront; } set { stepCalcPointInFront = value; RaisePropertyChanged(); } }
        double stepCalcPointInFront;

        /// <summary>
        /// Толщина линии фрона
        /// </summary>
        public double LineFrontWidth { get; set; }
        public string FrontLineLayer { get; set; }
        public ObjectId FrontLineLayerId { get; set; }

        public static FrontOptions Default ()
        {
            var opt = new FrontOptions {
                StepCalcPointInFront = 0.1,
                LineFrontWidth = 3,
                FrontLineLayer = "sapr_ins_front"
            };
            return opt;
        }

        public System.Drawing.Color GetFrontColor (InsRequirementEnum insValue)
        {
            switch (insValue)
            {
                case InsRequirementEnum.None:
                    return System.Drawing.Color.Gray;
                case InsRequirementEnum.D:                    
                case InsRequirementEnum.C:
                    return System.Drawing.Color.Green;
                case InsRequirementEnum.B:
                    return System.Drawing.Color.Yellow;
                case InsRequirementEnum.A:
                    return System.Drawing.Color.Red;                
            }
            return System.Drawing.Color.Gray;
        }

        public DicED GetExtDic (Document doc)
        {
            throw new NotImplementedException();
        }
        public void SetExtDic (DicED dicEd, Document doc)
        {
            throw new NotImplementedException();
        }
        public List<TypedValue> GetDataValues (Document doc)
        {
            throw new NotImplementedException();
        }
        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Определение слоя для линий фронтов
        /// </summary>
        /// <param name="db"></param>
        public void DefineLayer (Database db)
        {
            var lt = db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable;
            if (!lt.Has(FrontLineLayer))
            {
                var layer = new LayerTableRecord();
                layer.Name = FrontLineLayer;
                lt.UpgradeOpen();
                lt.Add(layer);
                db.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(layer, true);
            }
            FrontLineLayerId = lt[FrontLineLayer];
        }
    }
}
