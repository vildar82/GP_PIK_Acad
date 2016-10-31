using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Фронтоны - модель расчета
    /// </summary>
    public class FrontModel : ModelBase
    {
        public FrontModel ()
        {

        }

        public InsModel Model { get; set; }
    }    
}
