using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Parkings;

namespace PIK_GP_Acad.Elements.Blocks.Parkings
{
    /// <summary>
    /// КП_Паркинг - здание
    /// </summary>
    public class ParkingBuilding : BlockBase, IParking
    {
        public const string BlockName = "КП_Паркинг";

        private const string ParamPlaces = "^МАШИНОМЕСТА";
        private const string ParamFloors = "^ЭТАЖНОСТЬ";

        public int Floors { get; set; }
        public int Places { get; set; }
        public int InvalidPlaces { get; set; }       

        public ParkingBuilding (BlockReference blRef, string blName) : base(blRef, blName)
        {
            Floors = GetPropValue<int>(ParamFloors, exactMatch: false);

            var valPlaces = GetPropValue<string>(ParamPlaces, exactMatch:false);
            var resPlaces = AcadLib.Strings.StringHelper.GetStartInteger(valPlaces);
            if (resPlaces.Success)
            {
                Places = resPlaces.Value;
            }
            else
            {
                AddError($"Не определено кол машиномест из параметра {ParamPlaces} = {valPlaces}");
            }
        }

        public void Calc ()
        {            
        }
    }
}
