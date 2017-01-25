using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Geometry;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Группа выбора - для расчета фронтонов
    /// Группа - соответствует Блоку в проекте
    /// </summary>
    public class FrontGroup : ModelBase, IExtDataSave, ITypedDataValues, IDisposable
    {
        private static Tolerance toleranceVertex = new Tolerance(1,1);

        public FrontGroup()
        {

        }

        public FrontGroup(Extents3d selReg, FrontModel front)
        {
            Front = front;
            SelectRegion = selReg;
            Name = DefineNewName();
            Options = new FrontGroupOptions();
        }        

        /// <summary>
        /// Модель
        /// </summary>
        public FrontModel Front { get; set; }
        /// <summary>
        /// Область на чертеже
        /// </summary>
        public Extents3d SelectRegion { get; set; }  
        /// <summary>
        /// Настройки группы фронтов
        /// </summary>
        public FrontGroupOptions Options { get; set; }      
        /// <summary>
        /// Пользовательское имя группы
        /// </summary>
        public string Name { get { return name; } set { name = value; RaisePropertyChanged(); } }
        string name;

        /// <summary>
        /// Высота расчета фронтов
        /// </summary>
        public double FrontHeight { get { return frontHeight; }
            set { frontHeight = value; RaisePropertyChanged(); OnFrontHeightChanged(); } }
        double frontHeight;

        /// <summary>
        /// Дома
        /// </summary>
        public ObservableCollection<House> Houses { get { return houses; } set { houses = value; RaisePropertyChanged(); } }
        ObservableCollection<House> houses = new ObservableCollection<House>();
        /// <summary>
        /// Включение/отключение визуализации расчета фронтонов
        /// </summary>
        public bool IsVisualFrontOn { get { return isVisualFrontOn; }
            set { isVisualFrontOn = value; OnIsVisualIllumsChanges(); RaisePropertyChanged(); } }        
        bool isVisualFrontOn;        

        public string Info { get { return info; } set { info = value; RaisePropertyChanged(); } }
        string info;

        public bool IsExpanded { get { return isExpanded; } set { isExpanded = value; OnIsExpandedChanged(); RaisePropertyChanged(); } }
        bool isExpanded;

        ///// <summary>
        ///// Индентификатор группы (блока) в базе
        ///// </summary>
        //public int GroupId { get; set; }        

        /// <summary>
        /// Новая группа фронтонов
        /// </summary>
        /// <param name="selReg">Границы на чертеже</param>        
        public static FrontGroup New (Extents3d selReg, FrontModel front)
        {
            var frontGroup = new FrontGroup(selReg, front);
            return frontGroup;
        }
        public static FrontGroup New (DicED dicGroup, FrontModel front)
        {            
            var group = new FrontGroup();
            group.Front = front;
            group.SetExtDic(dicGroup, null);
            return group;
        }

        /// <summary>
        /// Обновоение расчета фронтонов и визуализации если она включена
        /// </summary>
        public void Update ()
        {
            //Document doc = Front.Model.Doc;
            //Database db = Front.Model.Doc.Database;
            //using (doc.LockDocument())
            //using (var t = db.TransactionManager.StartTransaction())
            //{
                //Front.InitToCalc();
                // Определение домов
                UpdateHouses();

            //    t.Commit();
            //}
        }        

        private void OnIsExpandedChanged ()
        {
            if (IsExpanded)
            {
                Show();
            }
        }
        
        private void OnIsVisualIllumsChanges ()
        {
            if (houses == null) return;
            // Включение/выключение визуализации фронтов во всех домах
            foreach (var item in Houses)
            {
                item.IsVisualFront = IsVisualFrontOn;
            }            
        }

        private void OnFrontHeightChanged()
        {
            foreach (var item in Houses)
            {
                item.FrontHeight = FrontHeight;
            }
        }

        /// <summary>
        /// Показать область группы на чертеде
        /// </summary>
        public void Show ()
        {
            if ((short)Application.GetSystemVariable("TILEMODE") == 0) return;
            var ed = Front?.Model?.Doc?.Editor;
            if (ed == null) return;
            ed.Zoom(SelectRegion);
        }

        public string DefineNewName ()
        {
            int index = Front?.Groups.Count +1 ?? 1;
            var name = "Группа " + index;
            return name; 
        }

        /// <summary>
        /// Обновление определения домов
        /// </summary>
        private void UpdateHouses ()
        {
            // Как сохранить предыдущие данные домов
            DisposeHouses();

            var oldHouses = Houses.ToList();

            // Считываение домов с чертежа в заданной области
            using (var scope = Front.Model.Map.GetScope(SelectRegion))
            {
                var houses = CreateHouses(scope);
                Houses = new ObservableCollection<House>(houses);
                foreach (var house in houses)
                {
                    // найти этот дом в старых домах
                    var oldHouse = oldHouses?.Find(h=>h.Name.Equals(house.Name));
                    if (oldHouse != null)
                    {
                        house.SetDataFromOldHouse(oldHouse);
                    }
                    else
                    {
                        house.IsVisualFront = true;
                    }
                    house.Update();
                }
            }
        }        

        /// <summary>
        /// Определение домов в выбранной области
        /// </summary>        
        public List<House> CreateHouses (Scope scope)
        {
            var houses = new List<House>();
            // Определение домов
            var projectBuildings = scope.Buildings;//.Where(b => b.Building.IsProjectedBuilding);
            foreach (var building in projectBuildings)
            {
                // Дом из блок-секций
                if (!FindHouse(ref houses, building))
                {
                    var house = new House(this);
                    house.Sections.Add(building);
                    houses.Add(house);
                }
            }
            // Для каждого дома - создание общей полилинии
            int countHouse = 1;
            foreach (var house in houses)
            {
                house.FrontGroup = this;
                house.DefineContour();
                // Заполнение оставшихся свойств дома
                house.DefineName(countHouse);
                countHouse++;
            }
            return houses;
        }        

        private bool FindHouse (ref List<House> houses, MapBuilding building)
        {            
            var findHouses = new List<House>();
            using (var offset = building.Contour.Offset(1, OffsetSide.Out).First())
            {
                foreach (var house in houses)
                {
                    foreach (var blInHouse in house.Sections)
                    {
                        using (var ptsIntersect = new Point3dCollection())
                        {
                            offset.IntersectWith(blInHouse.Contour, Intersect.OnBothOperands, ptsIntersect, IntPtr.Zero, IntPtr.Zero);
                            if (ptsIntersect.Count > 0)
                            {
                                findHouses.Add(house);
                                // Усреднение полилиний
                                foreach (var item in house.Sections)
                                {
                                    var contourItem = item.Contour;
                                    building.Contour.AverageVertexes(ref contourItem, toleranceVertex, true);
#if TEST
                                    //EntityHelper.AddEntityToCurrentSpace((Polyline)building.Contour.Clone());
                                    //EntityHelper.AddEntityToCurrentSpace((Polyline)contourItem.Clone());
#endif
                                }
                                break;
                            }
                        }
                    }
                }
            }
            if (findHouses.Any())
            {
                if (findHouses.Skip(1).Any())
                {
                    // Объединение нескольких домов в один общий
                    var bls = findHouses.SelectMany(s => s.Sections).ToList();
                    bls.Add(building);
                    var house = new House(this);
                    house.Sections = new ObservableCollection<MapBuilding>(bls);
                    houses.Add(house);
                    foreach (var h in findHouses)
                    {
                        houses.Remove(h);
                    }
                }
                else
                {
                    findHouses[0].Sections.Add(building);
                }
                return true;
            }
            return false;
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            var tvk = new TypedValueExtKit();
            tvk.Add("Name", Name);
            tvk.Add("IsVisualFrontOn", IsVisualFrontOn);
            return tvk.Values;            
        }
        public void SetDataValues (List<TypedValue> values, Document doc)
        {            
            var dictValues = values?.ToDictionary();
            Name = dictValues.GetValue("Name", "Группа");
            IsVisualFrontOn = dictValues.GetValue("IsVisualFrontOn", true);
        }
        public DicED GetExtDic (Document doc)
        {
            var dicGroup = new DicED();
            dicGroup.AddRec("GroupRec", GetDataValues(doc));
            dicGroup.AddInner("SelectRegion", GetExtDicSelectRegion());
            dicGroup.AddInner("Options", Options?.GetExtDic(doc));
            return dicGroup;
        }
        public void SetExtDic (DicED dicFront, Document doc)
        {
            SetDataValues(dicFront?.GetRec("GroupRec")?.Values, doc);
            SelectRegion = GetSelectRegionFromDict(dicFront?.GetInner("SelectRegion"));
            Options = new FrontGroupOptions();
            Options.SetExtDic(dicFront?.GetInner("Options"), doc);
        }
        private DicED GetExtDicSelectRegion ()
        {
            var dicSelReg = new DicED();
            var selRegTVs = new List<TypedValue> {
                // MinPt
                TypedValueExt.GetTvExtData(SelectRegion.MinPoint.X),
                TypedValueExt.GetTvExtData(SelectRegion.MinPoint.Y),
                // MaxPt
                TypedValueExt.GetTvExtData(SelectRegion.MaxPoint.X),
                TypedValueExt.GetTvExtData(SelectRegion.MaxPoint.Y),
            };
            dicSelReg.AddRec("SelRegRec", selRegTVs);
            return dicSelReg;
        }
        private Extents3d GetSelectRegionFromDict (DicED dicSelReg)
        {
            Extents3d resExt;
            var recSel = dicSelReg.GetRec("SelRegRec");            
            if (recSel!= null && recSel.Values!= null && recSel.Values.Count == 4)
            {
                int index = 0;
                var minPtX = TypedValueExt.GetTvValue<double>(recSel.Values[index++]);
                var minPtY = TypedValueExt.GetTvValue<double>(recSel.Values[index++]);
                var maxPtX = TypedValueExt.GetTvValue<double>(recSel.Values[index++]);
                var maxPtY = TypedValueExt.GetTvValue<double>(recSel.Values[index++]);

                resExt = new Extents3d(new Point3d(minPtX, minPtY, 0), new Point3d (maxPtX, maxPtY, 0));
            }
            else
            {
                resExt = new Extents3d();
            }
            return resExt;
        }

        public void ClearVisual()
        {
            foreach (var item in Houses)
            {
                item.ClearVisual();
            }
        }

        public void UpdateVisual ()
        {
            if (Houses != null)
            {
                foreach (var item in houses)
                {
                    item.UpdateVisual();
                }
            }
        }

        private void DisposeHouses()
        {
            foreach (var item in Houses)
            {
                item.DisposeContour();
                item.Dispose();
            }
        }

        public void Dispose ()
        {
            DisposeHouses();
        }
    }
}
