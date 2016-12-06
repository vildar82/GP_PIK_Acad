using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation.Services.Export
{
    /// <summary>
    /// Экспорт группы домов - фронтов инсоляции
    /// </summary>
    public class ExportFrontGoup
    {
        private FrontGroup front;        

        public ExportFrontGoup(FrontGroup front)
        {
            this.front = front;
        }

        /// <summary>
        /// Получение экспортных данных инсоляции фронтов группы домов (одного расчета Жуков)
        /// </summary>        
        public ExportInsData GetExportInsData ()
        {
            // Преобразование группы - перенос в 0,0 (как будет в Excel)
            var housesTrans = Transformation();
            if (housesTrans == null)
            {
                return null;
            }
            var resExportData = new ExportInsData(front, housesTrans);            
            return resExportData;
        }

        /// <summary>
        /// Преобразование группы - перенос в 0,0 (как будет в Excel)
        /// </summary>
        private List<HouseTransform> Transformation()
        {
            // Преобразование домов
            var housesTrans = new List<HouseTransform>();
            foreach (var item in front.Houses)
            {
                if (item.HouseId == 0) continue;
                var houseTrans = new HouseTransform(item);
                housesTrans.Add(houseTrans);
                // Нормализация дома - приведение к ортогональному виду (минимальный поворот до ортогональности вокруг точки центра дома)
                houseTrans.Normalize();                
            }
            if (!housesTrans.Any())
            {
                return null;
            }

            // Перенос группы домов в точку 0,0 (4 четверть)
            MoveToZero(housesTrans);

            // определение номеров столбцов и рядов ячеек инсоляции
            DefineNumCells(housesTrans);

            return housesTrans;
        }

        /// <summary>
        /// Определение номеров строк и столбцов ячеек инсоляции домов
        /// </summary>        
        private void DefineNumCells(List<HouseTransform> houses)
        {
            foreach (var house in houses)
            {
                house.DefineNumCells();
            }
        }


        /// <summary>
        /// Перенос группы домов в точку 0,0 (в 4 четверть)
        /// </summary>        
        private void MoveToZero(List<HouseTransform> houses)
        {
            // Граница группы
            var extGroup = GetExtents(houses);
            // вектор переноса (левый верхний угол границы в 0,0)
            var vecMove = Point2d.Origin - new Point2d(extGroup.MinPoint.X, extGroup.MaxPoint.Y);
            var matMove = Matrix2d.Displacement(vecMove);
            foreach (var house in houses)
            {
                house.Trans(matMove);
            }
        }

        /// <summary>
        /// Определение границ домов по точкам всех ячеек инсоляции
        /// </summary>        
        private Extents3d GetExtents(List<HouseTransform> houses)
        {
            var cells = houses.SelectMany(s => s.Cells);
            var ext = new Extents3d();
            foreach (var item in cells)
            {
                ext.AddPoint(item.PtCenter.Convert3d());
            }
            return ext;
        }
    }
}
