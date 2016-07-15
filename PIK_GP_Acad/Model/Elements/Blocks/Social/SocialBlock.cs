using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Elements.Blocks.Social
{
    /// <summary>
    /// Блок социального назначения - СОШ, ДОШ и т.п
    /// </summary>
    public abstract class SocialBlock : BlockBase, IElement
    {
        /// <summary>
        /// Этажей
        /// </summary>
        public int Floors { get; set; }
        /// <summary>
        /// Тип - "ДОО на 100 мест".
        /// </summary>
        public string Type { get; set; }
        public SocialBlock (BlockReference blRef, string blName) : base(blRef, blName)
        {
            Type = GetPropValue<string>("^ТИП", exactMatch: false);
            Floors = GetPropValue<int>("^ЭТАЖНОСТЬ", exactMatch: false);
        }

        protected int GetPlaces (string paramName)
        {
            int value =0;
            string input = GetPropValue<string>(paramName, exactMatch: false); // 100 круглый            
            var resInt = AcadLib.Strings.StringHelper.GetStartInteger(input);
            if (resInt.Success)
            {
                value = resInt.Value;
            }
            else
            {
                AddError($"Не определено '{paramName}' из параметра - {input}");
            }
            return value;
        }
    }
}
