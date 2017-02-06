using AcadLib.DB;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using AcadLib.Errors;
using Autodesk.AutoCAD.Geometry;

namespace Test_GP_Acad.Tests.Insolation.Compare
{
    public class InsObjects
    {
        List<EntityInfo> ents;


        public InsObjects()
        {
            ents = new List<EntityInfo>();
        }

        public static bool IsInsObject(Entity ent)
        {
            return Regex.IsMatch(ent.Layer, "ins_sapr_angle|ins_sapr_tree|ins_sapr_front|ins_sapr_place", RegexOptions.IgnoreCase);
        }

        public void Add(Entity ent)
        {
            ents.Add(new EntityInfo(ent));
        }

        public void Difference(InsObjects ins, string objectDiffsMsg, System.Drawing.Icon icon)
        {
            var diffs = ents.Except(ins.ents);
            foreach (var item in diffs)
            {
                Inspector.AddError($"{objectDiffsMsg} - {item.ToString()}", item.Extents, Matrix3d.Identity, icon);
            }            
        }
    }
}