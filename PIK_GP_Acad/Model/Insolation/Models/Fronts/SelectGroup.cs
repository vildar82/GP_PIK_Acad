using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation.Models
{
    public class SelectGroup
    {
        Document doc;
        Editor ed;
        public SelectGroup(Document doc)
        {
            this.doc = doc;
            ed = doc.Editor;
        }

        public Extents3d Select ()
        {
            var resSelReg = PromptSelectRegion();
            return resSelReg;
        }

        private Extents3d PromptSelectRegion ()
        {
            var resPt = ed.GetPoint("\nПервый угол области:");
            if (resPt.Status != PromptStatus.OK)
            {
                throw new AcadLib.CancelByUserException();
            }
            var resPt2 = ed.GetCorner("\nВторой угол:", resPt.Value);
            if (resPt2.Status != PromptStatus.OK)
            {
                throw new AcadLib.CancelByUserException();
            }
            var pt1 = resPt.Value.Trans(ed, CoordSystem.UCS, CoordSystem.WCS);
            var pt2 = resPt2.Value.Trans(ed, CoordSystem.UCS, CoordSystem.WCS);
            var ext = new Extents3d(pt1, pt2);
            return ext;
        }
    }
}
