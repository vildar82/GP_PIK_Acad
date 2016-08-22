using NUnit.Framework;
using PIK_GP_Acad.Model.Insolation.ShadowMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation;
using PIK_GP_Acad.Elements.Buildings;

namespace PIK_GP_Acad.Model.Insolation.ShadowMap.Tests
{
    [TestFixture()]
    public class ShadowServiceFactoryTests
    {
        [Test()]
        public void CreateTest ()
        {
            MoscowOptions opt = new MoscowOptions();
            Map map = null;
            var shadowService = ShadowServiceFactory.Create(opt, map);

            IBuilding building = null;
            var shadow = shadowService.Calc(building);

            Assert.IsNotNull(shadowService);
        }
    }
}