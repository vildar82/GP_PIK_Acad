using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Catel;
using Catel.Data;
using Catel.IoC;
using Catel.Services;
using Catel.Windows;
using PIK_GP_Acad.Insolation.Options;
using PIK_GP_Acad.Insolation.UI;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Общий сервис инсоляции для всех документов
    /// </summary>
    public static class InsService
    {        
        static Dictionary<Document, InsModel> insModels = new Dictionary<Document, InsModel>();
        static InsServicePallete palette;
        static InsViewModel insViewModel;
        public static IMessageService MessageService { get; private set; }
        public static IUIVisualizerService UIVisualizerService { get; private set; }
        public static ISettings Settings { get; private set; }

        static InsService()
        {            
            // Регистрация валидатора Catel.Extensions.FluentValidation
            ServiceLocator.Default.RegisterType<IValidatorProvider, FluentValidatorProvider>();
            MessageService = ServiceLocator.Default.ResolveType<IMessageService>();
            UIVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();

            Settings = new Settings();
            Settings.Load();
        }

        public static void Start (Document doc)
        {            
            Application.DocumentManager.DocumentActivated += (o, e) => ChangeDocument(e.Document);
            ChangeDocument(doc);
            palette.Visible = true;           
        }        

        public static void Stop ()
        {
            Application.DocumentManager.DocumentActivated -= (o,e) => ChangeDocument(e.Document);
            Settings.Save();
            palette.Visible = false;
            palette = null;
            insModels = null;
            insViewModel = null;
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

        private static void ChangeDocument (Document doc)
        {
            InsModel model;
            if (!insModels.TryGetValue(doc, out model))
            {
                model = new InsModel(doc);                
                insModels.Add(doc, model);
            }

            if (palette == null)
            {
                var insControl = new InsView();
                insControl.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
                insViewModel = new InsViewModel(model);
                insControl.DataContext = insViewModel;
                palette = new InsServicePallete(insControl);                
            }
            else
            {
                insViewModel.InsModel = model;
            }            
        }

        private static void Dispatcher_UnhandledException (object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageService.ShowAsync(e.Exception.Message, "", MessageButton.OK, MessageImage.Error);
        }
    }
}
