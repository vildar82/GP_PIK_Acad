﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Настройки инсоляционного требования
    /// </summary>
    public class InsRequirement
    {
        public InsRequirementEnum Type { get; set; }
        public Color Color { get; set; }

        public static string GetTypeString (InsRequirementEnum type)
        {
            var converter = new Catel.MVVM.ObjectToDisplayNameConverter();
            return (string)converter.Convert(type, typeof(string), null, null);
        }
    }
}