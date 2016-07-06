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

        public void Select (bool withRegions)
        {
            // Запрос региона
            if (withRegions)
                Estimate = PromptRegion();
            else
                Estimate = new EstimateMoscowRegion();
                            
            PromptSelectionResult res;
            var prOpt = new PromptSelectionOptions();            
            prOpt.MessageForAdding = "\nВыбор блоков блок-секций. ";
            prOpt.MessageForRemoval = "\nИсключено из набора";
            res = _doc.Editor.GetSelection(prOpt);
                
            if (res.Status != PromptStatus.OK)
            {
                throw new Exception("Отменено пользователем.");
            }

            

            IdsBlRefSections = new List<ObjectId>();
            foreach (ObjectId idEnt in res.Value.GetObjectIds())
            {
                var blRef = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                if (blRef == null) continue;
                var name = blRef.GetEffectiveName();
                if (SectionService.IsBlockNameSection(name))
                {
                    IdsBlRefSections.Add(idEnt);
                }
            }
        }

        private Estimate PromptRegion ()
        {
            Estimate res = null;
            var prOpt = new PromptKeywordOptions("\nВыбор региона:");
            var keys = string.Empty;
            prOpt.Keywords.Add("Ekb");
            prOpt.Keywords.Add("Msk");
            prOpt.Keywords.Add("mO");
            prOpt.Keywords.Default = "mO";            

            //keys = prOpt.Keywords.GetDisplayString(false);

            var prRes = _doc.Editor.GetKeywords(prOpt);

            if (prRes.Status != PromptStatus.OK)
            {
                throw new Exception(AcadLib.General.CanceledByUser);
            }
            res = Estimate.GetEstimate(prRes.StringResult);
            return res;
        }
    }
}