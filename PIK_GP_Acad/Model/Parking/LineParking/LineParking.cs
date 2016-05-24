using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Parking
{
    /// <summary>
    /// Линия парковки
    /// </summary>
    public class LineParking
    {
        public ObjectId IdBlRef { get; set; }
        /// <summary>
        /// Ширина одного парковочного места
        /// </summary>
        public double WidthOnePlace { get; set; }
        /// <summary>
        /// Длина линии парковки
        /// </summary>
        public double Length { get; set; }
        /// <summary>
        /// Угол парковочного места
        /// </summary>
        public double Angle { get; set; }       
        /// <summary>
        /// Парковка для инвалидов
        /// </summary>
        public bool IsInvalid { get; set; } 
        /// <summary>
        /// Кол машиномест в парковке
        /// </summary>
        public double Places { get; set; }

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
                    case "Длина":
                        Length = getDouble(prop.Value.ToString());
                        break;
                    default:
                        break;
                }
            }
            // проверка определенных параметров
            return check();
        }

        public void Calc()
        {
            var b = WidthOnePlace / (Math.Sin(Angle.ToRadians()));
            Places = Math.Floor(Length / b);
        }

        private Result check()
        {
            string err = string.Empty;
            if (WidthOnePlace == 0)            
                err += " Длина одного парковочного места не определена.";
            if (Length == 0)
                err += " Длина линии парковки не определена.";
            if (Angle == 0)
                err += " Угол парковочных мест не определен.";

            if (string.IsNullOrEmpty(err))
                return Result.Ok();
            else
                return Result.Fail(err);
        }

        private void parseView(string value)
        {
            var splits = value.Split('х');
            if(splits.Length == 2)
            {
                WidthOnePlace = getDouble(splits[0]);
                var split1 = splits[1];
                Angle = getDouble(Regex.Match(split1, @"\d+").Value);
                IsInvalid = split1.Contains("инвалид", StringComparison.OrdinalIgnoreCase);
            }
        }

        private double getDouble(string value)
        {
            double res;
            double.TryParse(value, out res);
            return res;
        }
    }
}
