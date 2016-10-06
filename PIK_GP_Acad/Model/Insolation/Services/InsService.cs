using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;
using Catel;
using Catel.ApiCop;
using Catel.ApiCop.Listeners;
using Catel.Data;
using Catel.ExceptionHandling;
using Catel.IoC;
using Catel.Services;
using Catel.Windows;
using PIK_GP_Acad.Insolation;
using PIK_GP_Acad.Insolation.Models;
using PIK_GP_Acad.Insolation.UI;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Общий сервис инсоляции для всех документов
    /// </summary>
    public static class InsService
    {
        static Dictionary<InsRequirementEnum, InsRequirement> dictInsReq;        
        static Dictionary<Document, InsModel> insModels;
        static InsServicePallete palette;
        static InsViewModel insViewModel;
        static InsView insView;        

        public static ISettings Settings { get; private set; }

        static InsService()
        {
#if DEBUG
            Catel.Logging.LogManager.AddDebugListener();
#endif
            Catel.Logging.LogManager.LogMessage += LogManager_LogMessage;

            // Регистрация валидатора Catel.Extensions.FluentValidation
            ServiceLocator.Default.RegisterType<IValidatorProvider, FluentValidatorProvider>();

            Settings = new Settings();
            Settings.Load();
            dictInsReq = Settings.InsRequirements.ToDictionary(k => k.Type, v => v);            
        }

        public static void Start (Document doc)
        {
            if (doc == null) return;
            if (insModels == null)
                insModels = new Dictionary<Document, InsModel>();
            Application.DocumentManager.DocumentActivated += (o, e) => ChangeDocument(e.Document);
            Application.DocumentManager.DocumentToBeDestroyed += (o, e) => CloseDocument(e.Document);
            if(palette == null)
                ChangeDocument(doc);
            palette.Visible = true;           
        }

        public static void Stop ()
        {                    
                Application.DocumentManager.DocumentActivated -= (o, e) => ChangeDocument(e.Document);
                Application.DocumentManager.DocumentToBeDestroyed -= (o, e) => CloseDocument(e.Document);
                //Settings.Save();
                //palette.Visible = false;
                palette = null;
                insModels = null;
                insViewModel = null;
#if DEBUG
            var apiCopFilelistener = new TextFileApiCopListener("apiCopInsolationListener.txt");
            ApiCopManager.AddListener(apiCopFilelistener);
            ApiCopManager.WriteResults();
#endif
        }

        

        public static IInsCalcService GetCalcService (InsOptions options)
        {
            var insCalc = InsCalcServiceFactory.Create(options);
            return insCalc;
        }

        public static InsRequirement GetInsReqByEnum (InsRequirementEnum type)
        {
            var req = dictInsReq[type];
            return req;
        }

        /// <summary>
        /// Возвращает значение атрибута Catel.ComponentModel.DisplayName навешенного на этот объект
        /// </summary>        
        public static string GetDisplayName (object obj)
        {
            var converter = new Catel.MVVM.ObjectToDisplayNameConverter();
            return (string)converter.Convert(obj, typeof(string), null, null);
        }

        public static void ShowMessage (string msg, MessageImage img)
        {
            ServiceLocator.Default.ResolveType<IMessageService>().ShowAsync(msg, "Инсоляция", MessageButton.OK, img);
        }
        public static void ShowMessage (Exception ex, string msg)
        {
            ShowMessage($"{msg} \n\r {ex.Message}", MessageImage.Error);
        }

        private static void CloseDocument (Document doc)
        {
            if (doc == null) return;
            // Сохранить инсоляцию если она активирована для этого чертежа

            // Очистка объекта
            insModels.Remove(doc);
        }

        private async static void ChangeDocument (Document doc)
        {
            if (doc == null) return;

            InsModel insModel;
            if (!insModels.TryGetValue(doc, out insModel))
            {
                insModel = new InsModel(doc);
                insModels.Add(doc, insModel);
            }

            if (palette == null)
            {
                insViewModel = new InsViewModel(insModel);
                insView = new InsView(insViewModel);
                insView.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
                palette = new InsServicePallete(insView);
                palette.StateChanged += Palette_StateChanged;
            }
            else
            {
                if (await insViewModel.SaveViewModelAsync() == true)
                {
                    insViewModel.Model = insModel;
                }
            }
        }

        private static void Palette_StateChanged (object sender, Autodesk.AutoCAD.Windows.PaletteSetStateEventArgs e)
        {
            if (e.NewState == Autodesk.AutoCAD.Windows.StateEventIndex.Hide)
            {
                Stop();
            }
        }

        private static void Dispatcher_UnhandledException (object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowMessage(e.Exception.Message, MessageImage.Error);
        }        

        private static void LogManager_LogMessage (object sender, Catel.Logging.LogMessageEventArgs e)
        {
            if (e.LogEvent == Catel.Logging.LogEvent.Error)
                Logger.Log.Debug(e.Message);
        }
    }
}
