using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Parkings
{    
    public class Parking : IParking
    {
        public const string ParkingBlockName = "ГП_Парковка";

        public ObjectId IdBlRef { get; set; }

        public bool IsInvalid { get; set; }

        public double Places { get; set; }        

        public void Calc()
        {

        }

        public Result Define(BlockReference blRef)
        {
            IdBlRef = blRef.Id;
            foreach (DynamicBlockReferenceProperty prop in blRef.DynamicBlockReferencePropertyCollection)
            {
                switch (prop.PropertyName)
                {
                    case "Вид":
                        parseView(prop.Value.ToString());
                        break;                    
                    default:
                        break;
                }
            }
            // проверка определенных параметров
            return check();
        }

        private void parseView(string value)
        {
            Places = int.Parse(Regex.Match(value, @"\d+").Value);            
            IsInvalid = value.Contains("инв", StringComparison.OrdinalIgnoreCase);
        }

        private Result check()
        {
            string err = string.Empty;
            if (Places == 0)
                err += " Кол парковочных мест не определено.";            

            if (string.IsNullOrEmpty(err))
                return Result.Ok();
            else
                return Result.Fail(err);
        }
    }
}