﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Catel.Data;
using PIK_GP_Acad.Insolation.Options;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Модель инсоляции в привязке к документу
    /// </summary>
    public class InsModel : ModelBase
    {
        public Document Doc { get; set; }
        public IMap Map { get; set; }  

        public InsModel (Document doc)
        {
            Doc = doc;
            Options = new InsOptions();
            Tree = new TreeModel(this);    
        }

        /// <summary>
        /// Настройки инсоляции
        /// </summary>
        public InsOptions Options { get; set; }

        /// <summary>
        /// Расчет елочек
        /// </summary>
        public TreeModel Tree { get; set; }
    }
}
