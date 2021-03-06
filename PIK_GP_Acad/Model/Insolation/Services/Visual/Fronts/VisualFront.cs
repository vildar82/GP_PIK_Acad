﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    public class VisualFront : VisualTransient
    {
        public VisualFront(Document doc): base("ins_sapr_front")
        {

        }

        public List<FrontValue> FrontLines { get; set; }            

        public override List<Entity> CreateVisual ()
        {
            if (FrontLines == null) return null;
            return FrontLines.Select(s => (Entity)s.Line.Clone()).ToList();
        }

        public override void Dispose()
        {
            if (FrontLines == null) return;
            foreach (var item in FrontLines)
            {
                item?.Dispose();
            }
            FrontLines = null;
            base.Dispose();
        }
    }
}
