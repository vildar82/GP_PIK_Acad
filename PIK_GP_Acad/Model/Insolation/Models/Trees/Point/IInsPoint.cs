using System.Collections.Generic;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    public interface IInsPoint : IDboDataSave
    {
        double AngleEndOnPlane { get; set; }
        double AngleStartOnPlane { get; set; }
        InsBuilding Building { get; }
        int Height { get; set; }
        List<IIlluminationArea> Illums { get; set; }
        string Info { get; set; }
        InsValue InsValue { get; set; }        
        int Number { get; set; }
        IVisualService VisualPoint { get; set; }
        WindowOptions Window { get; set; }
        void Initialize (TreeModel treeModel);
        void Update ();
        void VisualOnOff (bool onOff, bool saveState);
        void Delete ();
    }
}