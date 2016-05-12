using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace PIK_GP_Acad.BlockSection
{
    public class SelectSection
    {
        private Document _doc;
        private string region = "Москва";
        public Estimate Estimate { get; set; }

        public SelectSection(Document doc)
        {
            _doc = doc;
        }

        public List<ObjectId> IdsBlRefSections { get; private set; }

        public void Select(bool withRegions)
        {
            var prOpt = new PromptSelectionOptions();
            var keys = string.Empty;
            if (withRegions)
            {
                prOpt.Keywords.Add("Ekb");
                keys = prOpt.Keywords.GetDisplayString(true);
                prOpt.KeywordInput += (o, e) => { region = e.Input; };
            }

            prOpt.MessageForAdding = "\nВыбор блоков блок-секций. " + keys;
            prOpt.MessageForRemoval = "\nИсключено из набора" + keys;
            
            var res = _doc.Editor.GetSelection(prOpt);            

            if (res.Status != PromptStatus.OK)
            {
                throw new Exception("Отменено пользователем.");
            }

            Estimate = Estimate.GetEstimate(region);

            IdsBlRefSections = new List<ObjectId>();
            foreach (ObjectId idEnt in res.Value.GetObjectIds())
            {
                var blRef = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                if (blRef == null) continue;
                var name = blRef.GetEffectiveName();
                if (name.StartsWith(Settings.Default.BlockSectionPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    IdsBlRefSections.Add(idEnt);
                }
            }
        }       
    }
}