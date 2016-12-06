﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services.Export
{
    /// <summary>
    /// Экспорт инсоляции в базу.
    /// Чертежу должен быть назначен проект.
    /// Группам фронтов - блоки по проекту.
    /// Домам - назначены корпуса из проекта.
    /// Экспортируются только назначенные по проекту группы (и корпуса)
    /// </summary>
    public class ExportToDB
    {
        private FrontModel frontModel;

        public ExportToDB (FrontModel frontModel)
        {
            this.frontModel = frontModel;
        }

        /// <summary>
        /// Экспорт расчета.
        /// </summary>
        public void Export ()
        {
            // Отбор групп которым присвоен идентификатор, и в группе всем корпусам назначен идентификаторы
            //List<FrontGroup> notIdentifiedGroups;
            List<FrontGroup> exportedGroups = GetExportedGroups();
            // Форма подтверждения экпортируемых и не экпортируемых групп(неидентифицированных)
            var exportGroupsVM = new ExportGroupsViewModel(exportedGroups);
            if (InsService.ShowDialog(exportGroupsVM) == true)
            {
                foreach (var item in exportedGroups)
                {
                    var exportGroup = new ExportFrontGoup(item);
                    var exportData = exportGroup.GetExportInsData();
                    if (exportData == null)
                    {
                        return;
                    }
#if TEST
                    exportData.ToExel(@"c:\temp\exportIns.xlsx");
#endif
                }
            }
        }

        private List<FrontGroup> GetExportedGroups ()
        {
            var exportedGroups = new List<FrontGroup>();
            //notIdentifiedGroups = new List<FrontGroup>();
            foreach (var group in frontModel.Groups)
            {                
                // Проидентифицированно, можно экспортировать
                exportedGroups.Add(group);
                //notIdentifiedGroups.Add(group);
            }
            return exportedGroups;
        }
    }
}
