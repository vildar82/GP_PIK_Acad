using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsServicePallete : PaletteSet
    {
        static Dictionary<Document, InsServiceViewModel> dictDocIns = new Dictionary<Document, InsServiceViewModel>();
        static InsServicePallete palette;
        static InsServiceView control;  
        public static InsServiceViewModel CurrentInsServiceViewModel { get; private set; }

        public InsServicePallete() : base("Инсоляция", new Guid("E0E995F6-3D01-4D19-A888-BD167F1B9B5E"))
        {
            Icon = Properties.Resources.Sun;
            MinimumSize = new System.Drawing.Size(100, 100);            
        }

        public static void Start (Document doc)
        {
            if (palette == null)
                palette = new InsServicePallete();
            palette.Visible = true;
            Application.DocumentManager.DocumentActivated += (o,e)=> ChangeDocument(e.Document);
            ChangeDocument(doc);
        }        

        private static void ChangeDocument (Document doc)
        {
            InsServiceViewModel view;              
            if (!dictDocIns.TryGetValue(doc, out view))
            {
                view = new InsServiceViewModel();     
                dictDocIns.Add(doc, view);
            }
            CurrentInsServiceViewModel = view;

            if (palette.Count == 0)
            {
                control = new InsServiceView(view);
                palette.AddVisual("", control);                
            }
            else
            {
                control.DataUpdate(view);
            }            
        }
    }
}
