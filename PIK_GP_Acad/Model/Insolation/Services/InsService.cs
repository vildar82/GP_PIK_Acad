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
        static Dictionary<Type, Type> views;
        static Dictionary<InsRequirementEnum, InsRequirement> dictInsReq;        
        static Dictionary<Document, InsModel> insModels;
        static InsServicePallete palette;
        static InsViewModel insViewModel;
        static InsView insView;        

        public static ISettings Settings { get; private set; }        

        static InsService()
        {
            Settings = new Settings();
            Settings.Load();
            dictInsReq = Settings.InsRequirements.ToDictionary(k => k.Type, v => v);
            views = GetViews();
        }

        /// <summary>
        /// Переключатель активации расчета
        /// Если для текущего документа есть модель, значит расчет включен
        /// Установка значение - включает/отключает расчет
        /// </summary>
        public static bool InsActivate {
            get {
                var insModel = GetInsModel(Application.DocumentManager.MdiActiveDocument);
                return insModel != null && !insModel.IsCleared;
            }
            set {
                ActivateIns(value);
                RaiseStaticPropertyChanged(nameof(InsActivate));
            }
        }

        public static bool? ShowDialog (ViewModelBase viewModel)
        {
            Type view;
            if (views.TryGetValue(viewModel.GetType(), out view))
            {
                var win = (System.Windows.Window)Activator.CreateInstance(view);
                win.DataContext = viewModel;
                return Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowModalWindow(win);
            }
            throw new Exception("Окно не определено - тип = " + viewModel.GetType());            
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
            //Application.DocumentWindowCollection.DocumentWindowActivated += DocumentWindowCollection_DocumentWindowActivated;
            

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
            foreach (var item in insModels)
            {
                item.Value.Dispose();
            }
            palette = null;
            insModels = null;
            insViewModel = null;

            InsPointDrawOverrule.Stop();
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
            var insModel = GetInsModel(doc);            
            InsActivate = (insModel != null && !insModel.IsCleared);
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
            var doc = Application.DocumentManager.MdiActiveDocument;                                    

            // Сохранение текущей модели (состояние контролов - особенность Catel)            
            if (insViewModel.Model!= null)
            {
                //insViewModel.SaveViewModelAsync();
                //insViewModel.Model.SaveIns();                
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
                else
                {
                    insModel.Update();
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
                    //// Удаление
                    //insModels.Remove(doc);
                    //insModel = null;
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
            if (doc == null || doc.IsDisposed) return null;
            InsModel res = null;            
            insModels.TryGetValue(doc, out res);
            return res;
        }

        private static Dictionary<Type, Type> GetViews ()
        {
            return new Dictionary<Type, Type> {
                { typeof(InsRegionViewModel),  typeof(InsRegionView)}
            };
        }
    }
}
