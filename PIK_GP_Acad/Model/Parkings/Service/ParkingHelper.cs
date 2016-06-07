using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Layers;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Parkings
{
    public static class ParkingHelper
    {
        private static LayerInfo LayerText = new LayerInfo("ГП_Парковки_Текст");
        /// <summary>
        /// Слой для текста подписи парковок
        /// </summary>
        public static ObjectId LayerTextId
        {
            get
            {
                return LayerText.CheckLayerState();
            }
        }        
    }
}
