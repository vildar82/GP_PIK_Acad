using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using PIK_DB_Projects;

namespace PIK_GP_Acad.Insolation.Services
{
    public static class DBService
    {
        public static ProjectDB FindProject (int id)
        {
            ProjectDB project = null;
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

        public static List<ProjectDB> GetProjects ()
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
    }
}
