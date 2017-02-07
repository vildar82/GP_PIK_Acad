using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Models;
using PIK_GP_Acad.Insolation.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using MicroMvvm;
using AcadLib.Errors;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Общий сервис инсоляции для всех документов
    /// </summary>
    public static class InsService
    {
        public const string PluginName = "ins";
        static Dictionary<Type, Type> dictVmViews;
        static Dictionary<InsRequirementEnum, InsRequirement> dictInsReq;        
        static Dictionary<Document, InsModel> insModels;
        static InsServicePallete palette;
        static InsViewModel insViewModel;
        static InsView insView;        

        public static ISettings Settings { get; private set; }     
        public static UserSettings UserSettings { get; set; }   

        static InsService()
        {
            //UserSettings = UserSettings.Load();
            Settings = new Settings();
            Settings.Load();
            dictInsReq = Settings.InsRequirements.ToDictionary(k => k.Type, v => v);            
            dictVmViews = GetViews();
        }        

        /// <summary>
        /// Переключатель активации расчета
        /// Если для текущего документа есть модель, значит расчет включен
        /// Установка значение - включает/отключает расчет
        /// </summary>
        public static bool InsActivate {
            get {
                var insModel = GetInsModel(Application.DocumentManager.MdiActiveDocument);
                return insModel != null && insModel.IsEnabled;
            }
            set {
                try
                {
                    ActivateIns(value);
                }
                catch(Exception ex)
                {
                    ShowMessage(ex, "Ошибка.");
                }
                RaiseStaticPropertyChanged(nameof(InsActivate));
            }
        }

        public static bool? ShowDialog (ViewModelBase viewModel)
        {
            Type view;
            if (dictVmViews.TryGetValue(viewModel.GetType(), out view))
            {
                var win = (Window)Activator.CreateInstance(view);
                win.DataContext = viewModel;
                return Application.ShowModalWindow(win);
            }
            throw new Exception("Окно не определено - тип = " + viewModel.GetType());            
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        public static void RaiseStaticPropertyChanged (string propName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propName));
        }

        public static void StartInsolationPalette (Document doc)
        {
            if (doc == null || doc.IsDisposed) return;
            if (insModels == null)
                insModels = new Dictionary<Document, InsModel>();

            Application.DocumentManager.DocumentToBeDeactivated += DocumentManager_DocumentToBeDeactivated;
            Application.DocumentManager.DocumentActivated += DocumentManager_DocumentActivated;
            Application.DocumentManager.DocumentToBeDestroyed += DocumentManager_DocumentToBeDestroyed;

            DbService.Init();
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

        private static void DocumentManager_DocumentToBeDestroyed (object sender, DocumentCollectionEventArgs e)
        {
            CloseDocument(e?.Document);
        }

        private static void DocumentManager_DocumentActivated (object sender, DocumentCollectionEventArgs e)
        {
            ChangeDocument(e?.Document);
        }

        public static void Stop ()
        {
            // TODO: Сохранение всех расчетов      
            try
            {
                Application.DocumentManager.DocumentToBeDeactivated -= DocumentManager_DocumentToBeDeactivated;
                Application.DocumentManager.DocumentActivated -= DocumentManager_DocumentActivated;
                Application.DocumentManager.DocumentToBeDestroyed -= DocumentManager_DocumentToBeDestroyed;
            }
            catch
            {
                // ignored
            }

            //UserSettings.Save();
            //Settings.Save();
            //palette.Visible = false;
            foreach (var item in insModels)
            {
                try
                {
                    //item.Value.SaveIns();
                    item.Value.Dispose();
                }
                catch
                {
                    // ignored
                }
            }
            //palette = null;
            //insModels = null;
            //insViewModel = null;
            //insView = null;
            InsPointDrawOverrule.Stop();
            //GC.Collect();
            Logger.Log.Info($"Закрытие палитры инсоляции.");
        }

        private static void DocumentManager_DocumentToBeDeactivated (object sender, DocumentCollectionEventArgs e)
        {
            //var insModel = GetInsModel(e?.Document);
            //insModel?.ClearVisual();
        }

        public static ICalcService GetCalcService (InsOptions options)
        {
            var insCalc = CalcServiceFactory.Create(options);
            return insCalc;
        }

        public static InsRequirement GetInsReqByEnum (InsRequirementEnum type)
        {
            var req = dictInsReq[type];
            return req;
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
            var insModel = GetInsModel(doc);
            insModel?.Dispose();
            insModels?.Remove(doc);
        }

        private static void ChangeDocument (Document doc)
        {            
            var insModel = GetInsModel(doc);            
            InsActivate = insModel != null && insModel.IsEnabled;
        }

        private static void Palette_StateChanged (object sender, Autodesk.AutoCAD.Windows.PaletteSetStateEventArgs e)
        {
            if (e.NewState == Autodesk.AutoCAD.Windows.StateEventIndex.Hide)
            {
                // Сохранить расчеты ???
                Stop();
            }
        }
        
        /// <summary>
        /// Включение отключение расчета для текущего документа
        /// </summary>
        /// <param name="onOff">Включение или выключение расчета</param>
        private static void ActivateIns (bool onOff)
        {
            Inspector.Clear();
            var doc = Application.DocumentManager.MdiActiveDocument;                                    
                        
            if (insViewModel == null)
            {
                return;
            }

            if (doc == null || doc.IsDisposed)
            {
                insViewModel.Model = null;
                return;
            }

            var insModel = GetInsModel(doc);

            // Включение расчета для текущего документа
            if (onOff)
            {
                if (insModel == null)
                {
                    // Новый расчет
                    // Загрузка сохраненного расчета в чертеже (если он есть)
                    insModel = InsModel.LoadIns(doc);
                    if (insModel == null)
                    {
                        // Создание нового расчета
                        insModel = new InsModel();                        
                    }
                    insModels.Add(doc, insModel);
                    // Инициализация расчета
                    //insModel.Initialize(doc);                    
                    //insModel.Map.UpdateVisual();// Т.к. расчет не обновляется, то визуализация домов на карте (без отдельной визуализации домов во фронтах.)
                    
                    // Не обновлять расчет - пусть вручную обновляют
                    //try
                    //{
                    //    insModel.Update();
                    //}
                    //catch(UserBreakException)
                    //{
                    //}
                    // лог включения инсоляции для текущего чертежа
                    Logger.Log.Info($"Включение расчета инсоляции для чертежа - {doc.Name}");
                }
                //else
                //{
                //    insModel.Initialize(doc);
                //    insModel.UpdateVisual();
                //}                
                insModel.IsEnabled = true;
                insModel.Update();                
            }
            // Отключение расчета для текущего документа
            else
            {   
                if (insModel != null)
                {
                    insModel.ClearVisual(); // Очистка визуализаций
                    insModel.IsEnabled = false;
                    // лог отключения инсоляции для текущего чертежа
                    Logger.Log.Info($"Отключение расчета инсоляции для чертежа - {doc.Name}");
                }
            }
            // Переключение на модель (или на null)                
            insViewModel.Model = insModel;
            Inspector.Show();
        }

        /// <summary>
        /// Поиск модели для документа
        /// </summary>
        /// <param name="doc">Документ или null, тогда текущий док)</param>
        /// <returns></returns>
        private static InsModel GetInsModel (Document doc)
        {
            if (doc == null || doc.IsDisposed) return null;
            InsModel res = null;            
            insModels?.TryGetValue(doc, out res);
            return res;
        }

        private static Dictionary<Type, Type> GetViews ()
        {
            return new Dictionary<Type, Type> {
                { typeof(InsOptionsViewModel),  typeof(InsOptionsView)},
                { typeof (TreeOptionsViewModel), typeof(TreeOptionsView) },
                {  typeof (InsPointViewModel), typeof(InsPointView)},
                { typeof(PlaceOptionsViewModel), typeof(PlaceOptionsView) },
                { typeof(Export.ExportGroupsViewModel), typeof(Export.ExportGroupsView) },
                { typeof(FrontGroupOptionsViewModel), typeof(FrontGroupOptionsView) }
            };
        }

        public static void ShowMessage (Exception ex, string msg)
        {
            if (ex is CancelByUserException) return;
            Logger.Log.Error(ex, msg);
            ShowMessage($"{msg}\n\r{ex.Message}", MessageBoxImage.Error);            
        }

        public static void ShowMessage (string msg, MessageBoxImage icon)
        {
            MessageBox.Show(msg, "Инсоляция", MessageBoxButton.OK, icon, MessageBoxResult.OK);
        }

        public static System.Drawing.Color ColorPicker (System.Drawing.Color current)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.AnyColor = true;
            colorDialog.FullOpen = true;
            colorDialog.Color = current;
            colorDialog.AllowFullOpen = true;
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return colorDialog.Color;
            }
            return current;
        }       
    }
}
