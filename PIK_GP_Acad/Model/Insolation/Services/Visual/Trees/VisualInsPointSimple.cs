﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.GraphicsInterface;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    class VisualInsPointSimple : VisualServiceBase, IVisualInsPointSimple
    {
        public void Visual (InsPoint insPoint)
        {
            visuals = new List<IVisual>();
            foreach (var item in insPoint.Illums)
            {
                visuals.Add(item);
            }
            Update();
        }       
    }
}
