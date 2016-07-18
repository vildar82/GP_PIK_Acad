using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.OD
{
    public interface IODRecord
    {
        ODParameter this[string name] { get; }        
        ObjectId IdEnt { get; set; }
        string TableName { get; set; }
        List<ODParameter> Parameters { get; set; }
        string GetInfo ();
    }
}