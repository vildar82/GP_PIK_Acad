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
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.FCS;
using NetLib;     

namespace PIK_GP_Acad.Elements
{
    public static class ElementFactory
    {
        public static Dictionary<string, Type> BlockTypes = new Dictionary<string, Type>()
            {
                { TowerKPBS.BlockName, typeof(TowerKPBS) },
                { BlockSectionGP.BlockNameMatch, typeof(BlockSectionGP) },
                { BlockSectionKP.BlockNameMatch, typeof(BlockSectionKP) },
                { BlockSectionPIK1KP.BlockNameMatch, typeof(BlockSectionPIK1KP) },
                { LineParking.BlockName, typeof(LineParking) },
                { Parking.BlockName, typeof(Parking) },
                { ParkingBuilding.BlockName, typeof(ParkingBuilding) },
                { KindergartenBlock.BlockName, typeof(KindergartenBlock) },
                { SchoolBlock.BlockName, typeof(SchoolBlock) }                
            };

        /// <summary>
        /// Создание объекта.
        /// Или блока,
        /// Или классификатора, если задан classService
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ent"></param>
        /// <param name="classService"></param>
        /// <returns></returns>
        public static T Create<T>(Entity ent, IClassTypeService classService = null) where T : class, IElement
        {            
            T res = default(T);
            IElement elem = null;
            if (ent == null) return res;

            // Блок
            if (ent is BlockReference)
            {
                var blRef = (BlockReference)ent;
                string blName = blRef.GetEffectiveName();
                var blockType = GetBlockType(blName);
                if (typeof(T).IsAssignableFrom(blockType))
                {
                    elem = (IElement)Activator.CreateInstance(blockType, blRef, blName);                    
                }
            }
            else if (ent is Curve || ent is Hatch)
            {
                // Классифицируемый объект
                elem = GetClassificator(ent, classService);
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
        private static IClassificator GetClassificator (Entity ent, IClassTypeService classService)
        {
            IClassificator res = null;            
            FCEntProps fcEntProps;
            if (FCService.GetEntityProperties(ent.Id, out fcEntProps))
            {
                ClassType clType;
                if (classService == null)
                {
                    clType = new ClassType(fcEntProps.Class, fcEntProps.Class, null, 0);
                }
                else
                {
                    clType = classService?.GetClassType(fcEntProps.Class);
                }

                // Если класс проектируемого здания или есть параметр высоты, то это здание ???!!! Сомнительно. Нужна более строгая идентификайция зданий
                var height = fcEntProps.GetPropertyValue<double>(Building.PropHeight, 0);
                if (clType.ClassName.EqualsIgroreCaseAndSpecChars(Building.ProjectedBuildingClassName) ||
                    height != 0)
                {
                    var building = new Building(ent, height,  fcEntProps.GetProperties(), clType);
                    res = building;
                }
                else
                {
                    double area = GetArea(ent, clType.UnitFactor, clType.ClassName);
                    if (area != 0)
                    {
                        var classificator = new Classificator(ent, clType, area);
                        res = classificator;
                    }
                }
            }                     
            return res;
        }

        private static double GetArea (Entity ent, double unitFactor=1, string tag = "")
        {
            double res = 0;            
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
                    Inspector.AddError($"Неподдерживаемый тип объекта - '{ent.GetType().Name}'. Классификатор '{tag}'. Слой '{ent.Layer}'",
                            ent, System.Drawing.SystemIcons.Error);
                }
            }
            catch { }

            if (res == 0)
            {
                Inspector.AddError($"Не определена площадь объекта '{tag}'. Слой '{ent.Layer}'", ent, System.Drawing.SystemIcons.Error);
            }

            return res;
        }
    }
}
