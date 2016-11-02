using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Группа выбора - для расчета фронтонов
    /// </summary>
    public class FrontGroup : ModelBase, IExtDataSave, ITypedDataValues
    {
        public FrontGroup()
        {

        }

        public FrontGroup(Extents3d selReg, FrontModel front)
        {
            Front = front;
            SelectRegion = selReg;
            Name = GenerateName();
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
        /// Пользовательское имя группы
        /// </summary>
        public string Name { get { return name; } set { name = value; RaisePropertyChanged(); } }
        string name;        

        /// <summary>
        /// Дома
        /// </summary>
        public ObservableCollection<House> Houses { get { return houses; } set { houses = value; RaisePropertyChanged(); } }
        ObservableCollection<House> houses;

        /// <summary>
        /// Включение/отключение визуализации расчета фронтонов
        /// </summary>
        public bool IsVisualFrontOn { get { return isVisualFrontOn; } set { isVisualFrontOn = value; RaisePropertyChanged(); } }
        bool isVisualFrontOn;

        /// <summary>
        /// Новая группа фронтонов
        /// </summary>
        /// <param name="selReg">Границы на чертеже</param>        
        public static FrontGroup New (Extents3d selReg, FrontModel front)
        {
            var frontGroup = new FrontGroup(selReg, front);
            return frontGroup;
        }

        /// <summary>
        /// Обновоение расчета фронтонов и визуализации если она вклбючена
        /// </summary>
        public void Update ()
        {
            Document doc = Front.Model.Doc;
            Database db = Front.Model.Doc.Database;
            using (doc.LockDocument())
            using (var t = db.TransactionManager.StartTransaction())
            {
                Front.InitToCalc();

                // Определение домов
                UpdateHouses();

                t.Commit();
            }
        }        

        public List<TypedValue> GetDataValues (Document doc)
        {
            throw new NotImplementedException();
        }

        public DicED GetExtDic (Document doc)
        {
            throw new NotImplementedException();
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            throw new NotImplementedException();
        }

        public void SetExtDic (DicED dicEd, Document doc)
        {
            throw new NotImplementedException();
        }

        private string GenerateName ()
        {
            int index = Front?.Groups.Count + 1 ?? 1;
            var name = "Группа " + index;
            return name; 
        }

        /// <summary>
        /// Обновление определения домов
        /// </summary>
        private void UpdateHouses ()
        {
            // Как сохранить предыдущие данные домов
            var oldHouses = Houses.ToList();

            // Считываение домов с чертежа в заданной области
            using (var scope = Front.Model.Map.GetScope(SelectRegion))
            {
                var houses = House.GetHouses(scope, this);
                Houses = new ObservableCollection<House>(houses);
                foreach (var house in houses)
                {
                    house.CalcFront();
                }
            }
        }
    }
}
