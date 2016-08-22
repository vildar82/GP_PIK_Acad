using System.Collections.Generic;
using PIK_GP_Acad.Elements.Buildings;

namespace PIK_GP_Acad.Model.Insolation.ShadowMap
{
    /// <summary>
    /// Тень от здания
    /// </summary>
    public class Shadow
    {
        public IBuilding Building { get; set; }
        public bool[] InsTime { get; set; }   
        
        public Shadow(IBuilding build)
        {
            Building = build;
        }     
    }
}