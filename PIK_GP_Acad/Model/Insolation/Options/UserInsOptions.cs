using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;

namespace PIK_GP_Acad.Insolation.Options
{
    public class UserInsOptions
    {
        //static string fileUserInsOpt = General.GetUserDataFile("Insolation", "UserInsOptions.xml");
        Document doc;
        public RegionOptions Region { get; set; }

        public UserInsOptions(Document doc)
        {
            this.doc = doc;
        }

        public static UserInsOptions Load (Document doc)
        {
            // xml
            // var usrOpt = AcadLib.Files.SerializerXml.Load<UserInsOptions>(fileUserInsOpt);
            // return usrOpt; 
            throw new NotImplementedException();                       
        }       

        public void Save()
        {
            // xml
            // AcadLib.Files.SerializerXml.Save(fileUserInsOpt, this);
            throw new NotImplementedException();
        }
    }
}
