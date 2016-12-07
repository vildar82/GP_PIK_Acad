using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Elements.InfraworksExport;
using PIK_GP_Acad.OD;
using PIK_GP_Acad.OD.Records;

namespace PIK_GP_Acad.Elements.Blocks.Social
{
    /// <summary>
    /// Блок детского сада.
    /// </summary>
    public class KindergartenBlock : SocialBuilding
    {
        public const string BlockName = "КП_ДОО";
        const string contourLayer = "_ГП_здания ДОО";        

        public KindergartenBlock (BlockReference blRef, string blName) : base(blRef, blName, contourLayer)
        {
            FriendlyTypeName = "Дет.сад";
        }
    }
}
