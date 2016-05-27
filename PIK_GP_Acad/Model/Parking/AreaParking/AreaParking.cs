using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PIK_GP_Acad.Parking
{
    /// <summary>
    /// Парковка свободной площади - по полилинии
    /// </summary>
    public class AreaParking    
    {
        const string extInnerDictName = "GP_AreaParking";
        const string recFloors = "Floors";
        AreaParkingService service;
        ObjectId idPl; 
        
        /// <summary>
        /// Этажность парковки
        /// </summary>
        public int Floors { get; set; }
        /// <summary>
        /// Площадь одного этажа
        /// </summary>
        public double Area { get; set; }
        /// <summary>
        /// Машиномест
        /// </summary>
        public double Places { get; set; }
        

        public AreaParking (ObjectId idPolyline, AreaParkingService service)
        {             
            using (var t = service.Db.TransactionManager.StartTransaction())
            {
                idPl = idPolyline;
                var pl = idPl.GetObject(OpenMode.ForRead, false, true) as Polyline;
                Area = pl.Area;

                Floors = LoadFloors(pl);

                t.Commit();
            }
        }

        public void Calc()
        {
            if (Area == 0)
            {
                Places = 0;
                return;
            }
            if (Floors == 0)
            {
                Places = 0;
                return;
            }
            Places = Area * Floors / 40;
        }                                 
            
        private int LoadFloors(Polyline pl)
        {
            int res = 1;
            using (AcadLib.XData.EntDictExt extD = new AcadLib.XData.EntDictExt(pl, extInnerDictName))
            {
                var value = extD.Load<int>(recFloors);
                if (value != 0)
                {
                    res = value;
                }
            }
            return res;
        }        
    }
}
