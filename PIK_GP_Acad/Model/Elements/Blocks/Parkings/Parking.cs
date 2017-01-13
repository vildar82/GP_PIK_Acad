using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Blocks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Parkings;

namespace PIK_GP_Acad.Elements.Blocks.Parkings
{    
    public class Parking : BlockBase, IParking, IElement
    {
        public const string BlockName = "ГП_Парковка";        
        
        public ObjectId IdEnt { get; set; }
        public bool IsInvalid { get; set; }
        public int Places { get; set; }
        public int InvalidPlaces { get; set; }
        public string Layer { get { return BlLayer; } }

        public Parking (BlockReference blRef, string blName) : base(blRef, blName)
        {            
            IdEnt = blRef.Id;
            var view = GetPropValue<string>("Вид");
            parseView(view);            
            check();
        }

        public void Calc()
        {

        }

        private void parseView(string value)
        {
            IsInvalid = value.Contains("инв", StringComparison.OrdinalIgnoreCase);
            var places = int.Parse(Regex.Match(value, @"\d+").Value);
            if (IsInvalid)
            {
                InvalidPlaces = places;
            }
            else
            {
                Places = places;
            }            
        }

        private void check()
        {            
            if (Places == 0 && InvalidPlaces ==0)
            {
                AddError("Кол парковочных мест не определено.");
            }            
        }
    }
}