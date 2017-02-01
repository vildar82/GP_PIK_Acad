using AcadLib.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Services
{
    public static class PolylineSeparate
    {
        /// <summary>
        /// Определение отсекаемых полилиний от основной полинини по точкам пересечения.
        /// Оспользуется для определения теневых углов от собстаенного дома, когдв дом имеет сложную форму и имеет пересечения с линией тени от расчетной точки.
        /// </summary>
        /// <param name="pl">Основная полилиния</param>
        /// <param name="ptHead">Точка на главной полилинии - эта часть полилинии будет отделена в plHead</param>
        /// <param name="ptsSeparate">Точки пересечения</param>
        /// <param name="plHead">Полилиния головная - на которой расположена точка ptHead</param>
        /// <param name="plsSecant">Отсеченные полилинии точками пересечения</param>        
        public static bool Separate(this Polyline pl, Point3d ptHead, List<Point3d> ptsSeparate,
            out Polyline plHead, out List<Polyline> plsSecant)
        {
            plHead = null;
            plsSecant = new List<Polyline>();
            if (ptsSeparate == null || ptsSeparate.Count < 2) return false;

            var ptPrew = ptsSeparate[0];
            foreach (var ptItem in ptsSeparate.Skip(1))
            {
                var ptCentre = ptPrew + (ptItem - ptPrew) * 0.5;
                if (pl.IsPointInsidePolyline(ptCentre, false))
                {
                    // Точки отсекаемой части - между точками пересечения (prew и item)                    
                    int indexSecStart;
                    int indexSecEnd;
                    Point2d ptSecStart;
                    Point2d ptSecEnd;
                    var paramPrew = pl.GetParameterAtPoint(ptPrew);
                    var paramItem = pl.GetParameterAtPoint(ptItem);
                    var paramHead = pl.GetParameterAtPoint(ptHead);
                    if (paramPrew > paramItem)
                    {
                        var paramTemp = paramPrew;
                        paramPrew = paramItem;
                        paramItem = paramTemp;
                    }
                    // Если параметр главной точки попадает в диапазон между точками prew и item
                    if (paramHead > paramPrew && paramHead < paramItem)
                    {
                        // направление вершин от Item к Prew, через 0
                        indexSecStart = pl.NextVertexIndex((int)paramItem); // конечная точка сегмента точки перечечения item
                        indexSecEnd = (int)paramPrew; // начальная точка сегмента точки пересечения prew                                                                                                   
                        ptSecStart = ptItem.Convert2d();
                        ptSecEnd = ptPrew.Convert2d();
                    }
                    else
                    {
                        // направление вершин от Prew к Item
                        indexSecStart = Convert.ToInt32(Math.Ceiling(paramPrew)); // конечная точка сегмента точки перечечения prew
                        indexSecEnd = Convert.ToInt32(Math.Floor(paramItem)); // начальная точка сегмента точки пересечения item                          
                        ptSecStart = ptPrew.Convert2d();
                        ptSecEnd = ptItem.Convert2d();
                    }

                    if (indexSecStart == indexSecEnd)
                        return false;
                    // Удаление вершин отсекаемой части из полилинии дома и создание отсекаемой полилинии
                    plHead = (Polyline)pl.Clone();

                    var ptsSec = new List<Point2d>();
                    ptsSec.Add(ptSecStart);

                    var indexesToRemove = new HashSet<int>();

                    int index = indexSecStart;
                    var ptSec = pl.GetPoint2dAt(index);
                    ptsSec.Add(ptSec);
                    indexesToRemove.Add(index);
                    do
                    {
                        index = pl.NextVertexIndex(index);
                        ptSec = pl.GetPoint2dAt(index);
                        ptsSec.Add(ptSec);
                        indexesToRemove.Add(index);
                    } while (index != indexSecEnd);
                    ptsSec.Add(ptSecEnd);

                    foreach (var item in indexesToRemove.OrderByDescending(i => i))
                    {
                        plHead.RemoveVertexAt(item);
                    }

                    plsSecant.Add(ptsSec.CreatePolyline());
                }
            }
            return false;
        }
    }
}
