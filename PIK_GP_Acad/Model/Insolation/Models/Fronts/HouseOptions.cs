using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Models
{
    public class HouseOptions : FrontGroupOptions, IEquatable<HouseOptions>
    {
        public HouseOptions()
        {

        }
        public HouseOptions(FrontGroupOptions options)
        {
            Window = options.Window;
        }

        public bool Equals(HouseOptions other)
        {
            return Window.Equals(other.Window);
        }

        public bool Equals(FrontGroupOptions other)
        {
            return Window.Equals(other.Window);
        }

        public override int GetHashCode()
        {
            return Window?.GetHashCode() ?? 0;
        }
    }
}
