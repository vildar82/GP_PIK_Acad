using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation.Services
{
    public abstract class IllumAreaBase : IIlluminationArea
    {
        public double AngleEndOnPlane { get; set; }
        public double AngleStartOnPlane { get; set; }
        public int Time { get; set; }        

        public IllumAreaBase(double angleStart, double angleEnd)
        {
            AngleStartOnPlane = angleStart;
            AngleEndOnPlane = angleEnd;            
        }

        /// <summary>
        /// Объекдинение накладывающихся зон освещенности/тени
        /// </summary>                
        public static List<IIlluminationArea> Merge (List<IIlluminationArea> illums)
        {
            if (illums.Count == 0)
                return illums;

            List<IIlluminationArea> merged = new List<IIlluminationArea>();
            var sortedByStart = illums.OrderBy(o => o.AngleStartOnPlane).ToList();

            IIlluminationArea cur = sortedByStart[0];
            merged.Add(cur);
            foreach (var ilum in sortedByStart.Skip(1))
            {
                if (ilum.AngleStartOnPlane <= cur.AngleEndOnPlane)
                {
                    cur.AngleEndOnPlane = ilum.AngleEndOnPlane;
                }
                else
                {
                    merged.Add(ilum);
                    cur = ilum;
                }
            }
            return merged;
        }
    }
}
