using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.OD
{
    /// <summary>
    /// Описание таблицы данных Object Data
    /// </summary>
    public abstract class ODRecord : IODRecord
    {
        /// <summary>
        /// Объект к которому прикрепляется звапись OD
        /// </summary>
        public ObjectId IdEnt { get; set; }            
        /// <summary>
        /// Имя таблицы
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Параметры в таблице
        /// </summary>
        public List<ODParameter> Parameters { get; set; }        

        public ODRecord(string tableName, ObjectId idEnt)
        {
            TableName = tableName;
            IdEnt = idEnt;
        }

        public ODParameter this[string name] {
            get { return Parameters.Find(p => p.Name == name); }
        }

        public string GetInfo ()
        {
            var paramsInfo = string.Join(";", Parameters.Select(p=>p.GetInfo()));
            return $"Название = {TableName}, Параметры: {paramsInfo}";
        }
    }
}
