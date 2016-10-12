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
using AcadLib.Layers;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Parkings;

namespace PIK_GP_Acad.KP.Parking.Area
{
    /// <summary>
    /// Парковка свободной площади - по полилинии
    /// </summary>
    public class AreaParking    
    {
        const string extInnerDictName = "GP_AreaParking";
        const string recFloors = "Floors";
        const string recPlaceArea = "placeArea";
        public AreaParkingService Service { get; private set; }
        ObjectId idPl;

        /// <summary>
        /// Этажность парковки
        /// </summary>
        public int Floors { get; set; } = 1;
        /// <summary>
        /// Площадь одного этажа
        /// </summary>
        public double Area { get; set; }
        /// <summary>
        /// Машиномест
        /// </summary>
        public double Places { get; set; }
        /// <summary>
        /// Площадь одного парковочного места
        /// </summary>
        public double PlaceArea { get; set; } = 40;

        public AreaParking (ObjectId idPolyline, AreaParkingService service)
        {
            Service = service;
            using (var t = service.Db.TransactionManager.StartTransaction())
            {
                idPl = idPolyline;
                var pl = idPl.GetObject(OpenMode.ForRead, false, true) as Polyline;
                Area = Math.Round(pl.Area, 2);                
                Load(pl);
                t.Commit();
            }
        }

        /// <summary>
        /// Вставка текста
        /// </summary>
        public void InsertText()
        {            
            using (var t = Service.Db.TransactionManager.StartTransaction())
            {
                // Вставка текста
                DBText text = new DBText();
                text.SetDatabaseDefaults();
                text.Height = 2.5 * AcadLib.Scale.ScaleHelper.GetCurrentAnnoScale(Service.Db);

                //Слой                
                text.LayerId = ParkingHelper.LayerTextId;

                // стиль текста
                text.TextStyleId = Service.Db.GetTextStylePIK();

                text.TextString = $"М/м={Places}";

                var cs = Service.Db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                cs.AppendEntity(text);
                t.AddNewlyCreatedDBObject(text, true);

                if (AcadLib.Jigs.DragSel.Drag(Service.Ed, new[] { text.Id }, Point3d.Origin))
                {
                    SaveFloors();
                }                
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
            Places = Math.Round ( Area * Floors / PlaceArea, 1);
        }

        private void SaveFloors()
        {
            if (Floors != 0 && Floors != 1)
            {
                var pl = idPl.GetObject(OpenMode.ForWrite) as Polyline;
                using (AcadLib.XData.EntDictExt extD = new AcadLib.XData.EntDictExt(pl, extInnerDictName))
                {
                    extD.Save(recFloors, Floors);
                    extD.Save(recPlaceArea, PlaceArea);
                }
            }
        }

        private void Load(Polyline pl)
        {            
            using (AcadLib.XData.EntDictExt extD = new AcadLib.XData.EntDictExt(pl, extInnerDictName))
            {                 
                var value = extD.Load<int>(recFloors);
                if (value != 0)
                {
                    Floors = value;
                }
                var valuePlaceArea = extD.Load<double>(recPlaceArea);
                if (valuePlaceArea != 0)
                {
                    PlaceArea = valuePlaceArea;
                }
            }            
        }        
    }
}
