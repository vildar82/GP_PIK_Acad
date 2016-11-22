using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.OD.Records
{  
    /// <summary>
    /// Параметры Object Data для зданий
    /// </summary>
    public class ODBuilding : ODRecord
    {
        public const string BuildingLive = "Жилье";
        public const string BuildingSocial = "Социалка";
        public const string BuildingGarage = "Гаражи";

        // Параметры таблицы OD
        const string ParamTableName = "Здания";        
        const string ParamBuildingType = "Тип";
        const string ParamBuildingTypeDesc = "Тип здания: Жилье, Социалка, Гаражи";
        const string ParamHeight = "Высота";
        const string ParamHeightDesc = "Высота здания, м";        

        /// <summary>
        /// Запись OD Здания
        /// </summary>        
        public ODBuilding (ObjectId idEnt, BuildingType type, double height)  : base (ParamTableName, idEnt)
        {   
            Parameters = new List<ODParameter>() {
                new ODParameter(ParamBuildingType, DataType.Character, ParamBuildingTypeDesc, "Жилье") { Value = type.Name },
                new ODParameter(ParamHeight, DataType.Integer,ParamHeightDesc, 9) { Value = height }
            };
        }
        public static ODBuilding GetRecord (BlockBase block, ObjectId idEntBuilding, BuildingType type, double height)
        {            
            var IdPlContourInModel = block.CopyEntToModel(idEntBuilding.Database.CurrentSpaceId, idEntBuilding);
            ODBuilding od = new ODBuilding(IdPlContourInModel, type, height);
            return od;
        }
    }

    
}
