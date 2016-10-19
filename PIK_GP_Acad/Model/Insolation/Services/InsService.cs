using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
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

        /// <summary>
        /// Переключатель активации расчета
        /// Если для текущего документа есть модель, значит расчет включен
        /// Установка значение - включает/отключает расчет
        /// </summary>
        public static bool InsActivate {
            get { return GetInsModel(null) != null; }
            set {
                ActivateIns(value);
                RaiseStaticPropertyChanged(nameof(InsActivate));
            }
        }
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        public static void RaiseStaticPropertyChanged (string propName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propName));
        }

        public static void Start (Document doc)
        {
            if (doc == null || doc.IsDisposed) return;
            if (insModels == null)
                insModels = new Dictionary<Document, InsModel>();

            Application.DocumentManager.DocumentActivated += (o, e) => ChangeDocument(e.Document);
            Application.DocumentManager.DocumentToBeDestroyed += (o, e) => CloseDocument(e.Document);            

            InsPointDrawOverrule.Start();

            if (palette == null)
            {
                insViewModel = new InsViewModel();
                insView = new InsView(insViewModel);
                palette = new InsServicePallete(insView);
                palette.StateChanged += Palette_StateChanged;
            }
            palette.Visible = true;

            ChangeDocument(doc);               
        }

        

        public static void Stop ()
        {
            // TODO: Сохранение всех расчетов

            Application.DocumentManager.DocumentActivated -= (o, e) => ChangeDocument(e.Document);
            Application.DocumentManager.DocumentToBeDestroyed -= (o, e) => CloseDocument(e.Document);            
            //Settings.Save();
            //palette.Visible = false;
            palette = null;
            insModels = null;
            insViewModel = null;

            InsPointDrawOverrule.Stop();

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

        /// <summary>
        /// Поиск модели точки по инсоляционной точке на чертеже
        /// </summary>        
        /// <returns>Модель точки</returns>
        public static IInsPoint FindInsPoint (Point3d pt, Database db)
        {
            IInsPoint res = null;
            // Получение InsModel для соответствующего чертежа
            var insModel = insModels.FirstOrDefault(m => m.Key.Database == db).Value;
            if (insModel != null)
            {
                res =insModel.FindInsPoint(pt);
            }
            return res;
        }

        /// <summary>
        /// Закрытие расчета для документа
        /// </summary>        
        private static void CloseDocument (Document doc)
        {
            if (doc == null) return;
            // Очистка объекта
            insModels.Remove(doc);
        }

        private static void ChangeDocument (Document doc)
        {
            if (doc == null) return;
            var insModel = GetInsModel(doc);
            InsActivate = insModel != null;
        }

        private static void Palette_StateChanged (object sender, Autodesk.AutoCAD.Windows.PaletteSetStateEventArgs e)
        {
            if (e.NewState == Autodesk.AutoCAD.Windows.StateEventIndex.Hide)
            {
                // Сохранить расчеты ???
                Stop();
            }
        }

        private static void LogManager_LogMessage (object sender, Catel.Logging.LogMessageEventArgs e)
        {
            if (e.LogEvent == Catel.Logging.LogEvent.Error)
                Logger.Log.Debug(e.Message);
        }

        /// <summary>
        /// Включение отключение расчета для текущего документа
        /// </summary>
        /// <param name="onOff">Включение или выключение расчета</param>
        private static void ActivateIns (bool onOff)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null || doc.IsDisposed) return;

            var insModel = GetInsModel(doc);            

            // Сохранение текущей модели (состояние контролов - особенность Catel)            
            if (insViewModel.Model!= null)
            {
                insViewModel.SaveViewModelAsync();
                insViewModel.Model.SaveIns();                
            }

            // Включение расчета для текущего документа
            if (onOff)
            {
                if (insModel == null)
                {
                    // Загрузка сохраненного расчета в чертеже (если он есть)
                    insModel = InsModel.LoadIns(doc);
                    if (insModel == null)
                    {
                        // Создание нового расчета
                        insModel = new InsModel();
                    }
                    insModels.Add(doc, insModel);
                    // Инициализация расчета
                    insModel.Initialize(doc);                                                                   
                }                
            }
            // Отключение расчета для текущего документа
            else
            {
                if (insModel != null)
                {
                    // Сохранение расчета
                    //insModel.SaveIns();
                    insModel.Clear();
                    // Удаление
                    insModels.Remove(doc);
                    insModel = null;
                }
            }
            // Переключение на модель (или на null)                
            insViewModel.Model = insModel;
        }

        /// <summary>
        /// Поиск модели для документа
        /// </summary>
        /// <param name="doc">Документ или null, тогда текущий док)</param>
        /// <returns></returns>
        private static InsModel GetInsModel (Document doc)
        {
            InsModel res = null;
            if (doc == null)
            {
                doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return res;
            }
            insModels.TryGetValue(doc, out res);
            return res;
        }
    }
}
