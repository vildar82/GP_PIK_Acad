using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using PIK_DB_Projects;

namespace PIK_GP_Acad.Insolation.Services
{
    public static class DbService
    {
        public static ProjectMDM FindProject (int id)
        {
            ProjectMDM project = null;
            if (id != 0)
            {
                try
                {
                    var projects = MDMService.GetProjects();
                    project = projects.Find(p => p.Id == id);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, "MDMService.GetProjects();");
                }
            }
            return project;
        }

        public static List<ProjectMDM> GetProjects ()
        {
            try
            {
                return MDMService.GetProjects();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "MDMService.GetProjects();");
            }
            return null;
        }


        /// <summary>
        /// Получение корпусов для проекта.
        /// Корпус - объект МДМ у которого все вложенные объекты уровнем ниже Корпуса или нет вложенных объектов
        /// </summary>        
        public static List<ObjectMDM> GetHouses(ProjectMDM project)
        {
            if (project == null) return null;
            var objs = MDMService.GetHouses(project.Id);
            return objs;
        }
    }    
}
