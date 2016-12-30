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
        private static Dictionary<int, ProjectMDM> projects;
        
        public static void Init()
        {
            Task.Run(() =>
            {
                try
                {
                    projects = MDMService.GetProjects().ToDictionary(k => k.Id, v => v);
                }
                catch(Exception ex)
                {
                    Logger.Log.Error(ex, "DbService.Init() - MDMService.GetProjects().");
                }
            });
        }       

        public static ProjectMDM FindProject (int id)
        {
            ProjectMDM project = null;
            if (id != 0)
            {
                try
                {
                    if (projects == null)
                        projects = MDMService.GetProjects().ToDictionary(k => k.Id, v => v);
                    projects.TryGetValue(id, out project);
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
                if (projects == null)
                    projects = MDMService.GetProjects().ToDictionary(k => k.Id, v => v);
                return projects.Values.ToList();
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
