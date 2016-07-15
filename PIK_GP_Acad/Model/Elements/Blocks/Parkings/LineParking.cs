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
    /// <summary>
    /// Линия парковки
    /// </summary>
    public class LineParking : BlockBase, IParking, IElement
    {
        public const string BlockName = "ГП_Линия-Парковки";        
        
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
        public int Places { get; set; }
        public int InvalidPlaces { get; set; }

        public LineParking (BlockReference blRef, string blName) : base(blRef, blName)
        {
            var view = GetPropValue<string>("Вид");
            parseView(view);
            Length = GetPropValue<double>("Длина");
            check();
        }

        public void Calc()
        {
            var b = WidthOnePlace / (Math.Sin(Angle.ToRadians()));            
            var places =Convert.ToInt32(Math.Floor(Length / b));
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
            string err = string.Empty;
            if (WidthOnePlace == 0)            
                err += " Длина одного парковочного места не определена.";
            if (Length == 0)
                err += " Длина линии парковки не определена.";
            if (Angle == 0)
                err += " Угол парковочных мест не определен.";

            if (!string.IsNullOrEmpty(err))
            {
                AddError(err);
            }                
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
