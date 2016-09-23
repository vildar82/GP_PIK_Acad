using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Catel;
using Catel.Data;
using Catel.IoC;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Общий сервис инсоляции для всех документов
    /// </summary>
    public static class InsService
    {        
        static Dictionary<Document, InsModel> insModels = new Dictionary<Document, InsModel>();        

        static InsService()
        {
            // Регистрация валидатора Catel.Extensions.FluentValidation
            ServiceLocator.Default.RegisterType<IValidatorProvider, FluentValidatorProvider>();
        }

        /// <summary>
        /// Получение инсоляционной модели документа
        /// </summary>        
        public static InsModel GetIns (Document doc)
        {
            InsModel insModel;
            if (!insModels.TryGetValue(doc, out insModel))
            {
                insModel = new InsModel(doc);
                insModels.Add(doc, insModel);
            }
            return insModel;
        }
    }
}
