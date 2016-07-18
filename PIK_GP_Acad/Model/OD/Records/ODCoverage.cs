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
    /// Параметры Object Data для Покрытий
    /// </summary>
    public class ODCoverage : ODRecord
    {
        public const string CoverageSideWalk = "Тротуары с проездом";        

        const string ParamTableName = "Покрытия";        
        const string ParamCoverageType = "Тип";
        const string ParamCoverageTypeDesc = "Тип покрытия: Тротуары с проездом";

        /// <summary>
        /// Запись OD Покрытия
        /// </summary>        
        public ODCoverage (ObjectId idEnt, CoverageType type) : base (ParamTableName, idEnt)
        {
            Parameters = new List<ODParameter>() {
                new ODParameter(ParamCoverageType, DataType.Character,ParamCoverageTypeDesc, CoverageSideWalk) { Value = type.Name }                
            };            
        }

        public static List<ODCoverage> GetRecords (BlockBase block, string layer, CoverageType type)
        {
            List<ODCoverage> recs = new List<ODCoverage>();
            var btr = block.IdBtr.GetObject(OpenMode.ForRead) as BlockTableRecord;
            var plsCoverage = btr.GetObjects<Polyline>().
                    Where(p => p.Visible && p.Layer.Equals(layer, StringComparison.OrdinalIgnoreCase));
            foreach (var item in plsCoverage)
            {
                var idPlCoverage = block.CopyEntToModel(btr.Database.CurrentSpaceId, item.Id);
                ODCoverage odCoverage = new ODCoverage(idPlCoverage, type);
                recs.Add(odCoverage);
            }
            return recs;
        }
    }

    public class CoverageType
    {
        public static readonly CoverageType SideWalk = new CoverageType(ODCoverage.CoverageSideWalk);        

        public readonly string Name;       
                 
        private CoverageType (string name)
        {
            Name = name;            
        }
    }
}
