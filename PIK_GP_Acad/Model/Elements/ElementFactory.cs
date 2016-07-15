using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Elements.Blocks.BlockSection;
using PIK_GP_Acad.Elements.Blocks.Parkings;
using PIK_GP_Acad.Elements.Blocks.Social;
using PIK_GP_Acad.FCS;

namespace PIK_GP_Acad.Elements
{
    public static class ElementFactory
    {
        public static Dictionary<string, Type> BlockTypes = new Dictionary<string, Type>()
            {
                { TowerKPBS.BlockName, typeof(TowerKPBS) },
                { BlockSectionGP.BlockNameMatch, typeof(BlockSectionGP) },
                { BlockSectionKP.BlockNameMatch, typeof(BlockSectionKP) },                
                { LineParking.BlockName, typeof(LineParking) },
                { Parking.BlockName, typeof(Parking) },
                { ParkingBuilding.BlockName, typeof(ParkingBuilding) },
                { KindergartenBlock.BlockName, typeof(KindergartenBlock) },
                { SchoolBlock.BlockName, typeof(SchoolBlock) }                
            };

        public static T Create<T>(Entity ent, IClassTypeService classService = null) where T : class, IElement
        {
            T res = default(T);
            IElement elem = null;      
            
            // Блок
            if (ent is BlockReference)
            {
                var blRef = (BlockReference)ent;
                string blName = blRef.GetEffectiveName();
                var blockType = GetBlockType(blName);
                if (blockType != null)
                {
                    elem = (IElement)Activator.CreateInstance(blockType, blRef, blName);                    
                }
            }
            else
            {
                // Классифицируемый объект
                elem = GetClassificator(ent.Id, classService);
            }       

            if (elem?.Error != null)
            {
                Inspector.AddError(elem.Error);
                elem = null;
            }

            // Попытка приведения типа элемента к T
            res = elem as T;

            return res;
        }

        private static Type GetBlockType (string blName)
        {
            var res = BlockTypes.FirstOrDefault(t => Regex.IsMatch(blName, t.Key, RegexOptions.IgnoreCase));
            return res.Value;
        }

        /// <summary>
        /// !!! Перед использование не забудь вызвать первый раз FCService.Init()
        /// </summary>        
        public static IClassificator GetClassificator (ObjectId idEnt, IClassTypeService classService)
        {
            IClassificator res = null;
            KeyValuePair<string, List<FCProperty>> tag;
            if (FCService.GetTag(idEnt, out tag))
            {
                if (classService != null)
                {
                    var classType = classService.GetClassType(tag.Key);
                    if (classType != null)
                    {                        
                    }
                }
                double value = GetValue(idEnt, classType.UnitFactor, classType.ClassName);
                if (value != 0)
                {
                    res = new Classificator(idEnt, classType);
                }
            }
            return res;
        }

        private static double GetValue (ObjectId idEnt, double unitFactor=1, string tag = "")
        {
            double res = 0;
            var ent = idEnt.GetObject(OpenMode.ForRead);
            try
            {
                if (ent is Curve)
                {
                    var curve = ent as Curve;
                    res = curve.Area * unitFactor;
                }
                else if (ent is Hatch)
                {
                    var h = ent as Hatch;
                    res = h.Area * unitFactor;
                }
                else
                {
                    Inspector.AddError($"Неподдерживаемый тип объекта - {idEnt.ObjectClass.Name}. Классификатор - {tag}",
                            idEnt, System.Drawing.SystemIcons.Error);
                }
            }
            catch { }

            if (res == 0)
            {
                Inspector.AddError($"Не определена площадь объекта {tag}", idEnt, System.Drawing.SystemIcons.Error);
            }

            return res;
        }
    }
}
