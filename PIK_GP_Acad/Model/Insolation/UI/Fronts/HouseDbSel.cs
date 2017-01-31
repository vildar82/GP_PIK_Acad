using MicroMvvm;
using PIK_DB_Projects;
using PIK_GP_Acad.Insolation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PIK_GP_Acad.Insolation.UI
{
    /// <summary>
    /// Дом для связывания или связанный дом
    /// </summary>
    public class HouseDbSel : ModelBase, IEquatable<HouseDbSel>
    {
        private static Brush colorConnected = new SolidColorBrush(Colors.Green);
        private static Brush colorMoreConnections = new SolidColorBrush(Colors.Red);
        private static Brush colorFree = new SolidColorBrush(Colors.Transparent);

        private ObjectMDM objectMDM { get; set; }
        /// <summary>
        /// Связанные дома с этим объектом базы
        /// </summary>
        private HashSet<House> connectedHouses = new HashSet<House>();

        public HouseDbSel()
        {

        }

        public HouseDbSel(ObjectMDM obj)
        {
            objectMDM = obj;
            if (obj == null)
            {
                Id = 0;
                Name = "Нет";
                Color = colorFree;
                Status = "";
            }
            else
            {
                Id = obj.Id;
                Name = obj.FullName;
                Status="Не связанный дом";
            }
        }        
        
        public int Id { get; set; }
        public string Name { get; set; }
        public Brush Color { get { return color; } set { color = value; RaisePropertyChanged(); } }
        Brush color;
        public string Status { get { return status; } set { status = value; RaisePropertyChanged(); } }
        public bool IsFree { get { return connectedHouses.Count == 0; } }

        public static HouseDbSel Empty { get { return empty; } }
        static HouseDbSel empty;

        string status;

        public void Connect(House house)
        {
            if (objectMDM == null || house == null) return;
            if (connectedHouses.Add(house))
                DefineColorAndStatus();
        }

        public void Disconnect(House house)
        {
            if (objectMDM == null) return;
            connectedHouses.Remove(house);
            DefineColorAndStatus();
        }

        public void ClearConnections()
        {
            foreach (var item in connectedHouses)
            {
                item.SelectedHouseDb = null;
            }
            connectedHouses.Clear();
            DefineColorAndStatus();
        }

        private void DefineColorAndStatus()
        {
            if (objectMDM == null) return;
            if (connectedHouses.Count ==0)
            {
                Color = colorFree;
                Status = "Не связанный дом";                
            }
            else if (connectedHouses.Count>1)
            {
                Color = colorMoreConnections;
                Status = "Связан более чем с одним домом на чертеже";                
            }
            else
            {
                Color = colorConnected;
                Status = "Связанный дом";                
            }
        }

        public override int GetHashCode()
        {
            return Id;
        }
        public bool Equals(HouseDbSel other)
        {
            return Id == other?.Id;
        }        
    }
}
