using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using PIK_GP_Acad.Insolation;
using PIK_GP_Acad.Insolation.Models;
using PIK_GP_Acad.Insolation.Options;
using PIK_GP_Acad.Insolation.Services;

namespace Test_GP_Acad.Tests.Insolation
{
    public class TestInsolationService
    {
        /// <summary>
        /// Тест создания инсоляционнной моделя для документа
        /// </summary>        
        public void CreateInsModelForDoc (Document doc)
        {
            InsModel insModel = InsService.GetIns(doc);
            // Расчет елочек
            // Добавление расчетной точки
            insModel.Tree.AddPoint();      
        }
    }
}
