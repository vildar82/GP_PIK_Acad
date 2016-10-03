using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using PIK_GP_Acad.Insolation.Models;
using AcadLib;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Визуализация описания точки на чертеже
    /// </summary>
    public class VisualInsPointInfo : VisualServiceBase, IVisual
    {
        public InsPoint InsPoint { get; set; }
        public VisualInsPointInfo (InsPoint insPoint)
        {
            InsPoint = insPoint;
            visuals = new List<IVisual> { this };
        }

        public List<Drawable> CreateVisual ()
        {
            List<Drawable> draws = new List<Drawable>();
            // Кружок (положение точки)
            draws.Add(GetTarget());
            
            var ptText = InsPoint.Point + new Vector3d(0, 1, 0);
            var c = InsPoint.InsValue.Requirement.Color;
            // Макимальная непрерывная инсоляция
            draws.Add(GetText(ptText, InsPoint.InsValue.MaxContinuosTime.ToHours()+ "ч.", c, 0.5, AttachmentPoint.BottomCenter));
            // Тип требования
            ptText = ptText + new Vector3d(0,2,0);
            draws.Add(GetText(ptText,InsRequirement.GetTypeString(InsPoint.InsValue.Requirement.Type), c, 1, AttachmentPoint.BottomCenter));
            // Номер точки
            ptText = ptText + new Vector3d(0, 2, 0);
            draws.Add(GetText(ptText, InsPoint.Number.ToString(), c, 1.5, AttachmentPoint.BottomCenter));

            return draws;
        }

        private Drawable GetText (Point3d pos, string text, System.Drawing.Color c, double height, AttachmentPoint justify)
        {            
            DBText dbText = new DBText();
            dbText.SetDatabaseDefaults(HostApplicationServices.WorkingDatabase);
            dbText.Position = pos;
            dbText.TextString = text;
            dbText.Color = Color.FromColor( c);
            dbText.ColorIndex = 5;
            dbText.Height = height;
            dbText.Justify = justify;
            dbText.AlignmentPoint = pos;
            dbText.AdjustAlignment(HostApplicationServices.WorkingDatabase);
            return dbText;
        }

        private Drawable GetTarget ()
        {
            Circle c = new Circle(InsPoint.Point, Vector3d.ZAxis, 0.5);
            c.Color = Color.FromColor(System.Drawing.Color.Yellow);
            return c;
        }
    }
}
