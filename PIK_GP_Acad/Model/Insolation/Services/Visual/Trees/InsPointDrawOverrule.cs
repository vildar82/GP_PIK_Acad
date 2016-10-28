using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    public class InsPointDrawOverrule : DrawableOverrule
    {
        private static InsPointDrawOverrule _overrule;
        private static RXClass rxDbPoint = GetClass(typeof(DBPoint));

        public InsPointDrawOverrule()
        {
            SetXDataFilter(AcadLib.XDataExt.PikApp);
        }

        public static void Start()
        {
            if (_overrule == null)
            {
                Overruling = true;
                _overrule = new InsPointDrawOverrule();                
                AddOverrule(rxDbPoint, _overrule, true);
            }
        }

        public static void Stop ()
        {
            if (_overrule != null)
            {
                RemoveOverrule(rxDbPoint, _overrule);
                _overrule = null;
                Overruling = false;
            }
        }

        public override bool IsApplicable (RXObject overruledSubject)
        {            
            // Проверка точки - инсоляционная или нет
            var dPt = (DBPoint)overruledSubject;
            return InsPointHelper.IsInsPoint(dPt);            
        }

        public override int SetAttributes (Drawable drawable, DrawableTraits traits)
        {
            var dbPt = drawable as DBPoint;
            IInsPoint insPoint = InsService.FindInsPoint(dbPt.Position, dbPt.Database);
            if (insPoint != null && insPoint.InsValue != null)
            {
                traits.TrueColor = Color.FromColor(insPoint.InsValue.Requirement.Color).EntityColor;
                return 1;
            }            
            return base.SetAttributes(drawable, traits);            
        }

        public override bool WorldDraw (Drawable drawable, WorldDraw wd)
        {
            var dbPt = drawable as DBPoint;
            // Найти инсоляционную точку
            IInsPoint insPoint = InsService.FindInsPoint(dbPt.Position, dbPt.Database);
            if (insPoint != null && insPoint.VisualPoint != null)
            {                   
                var draws = insPoint.VisualPoint.CreateVisual();
                foreach (var item in draws)
                {
                    item.WorldDraw(wd);
                    item.Dispose();
                }                
            }
            return base.WorldDraw(drawable, wd);                        
        }
    }
}
