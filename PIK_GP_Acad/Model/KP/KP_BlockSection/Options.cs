using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;

namespace PIK_GP_Acad.KP.KP_BlockSection
{
    class Options
    {        
        static string FileXml = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder,
                                       @"ГП\KP_BlockSection.xml");        

        static Options _instance;
        public static Options Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }

        Options()
        {
        }

        [Category("Блок-секция")]
        [DisplayName("Имя блока секции")]
        [Description("Соответствие имени блока Блок-Секции. ^ГП_К_Секция - имя блока начинается с ГП_К_Секция. Регистр игнорируется.")]
        [DefaultValue("^ГП_К_Секция")]
        public string BlockSectionNameMatch { get; set; }

        [Category("Блок-секция")]
        [DisplayName("Атрибут этажности")]
        [Description("Тег атрибута Этажности в блоке Блок-секции")]
        [DefaultValue("ЭТАЖЕЙ")]
        public string BlockSectionAtrFloor { get; set; }

        void SetDefault()
        {
            BlockSectionNameMatch = "^ГП_К_Секция";
            BlockSectionAtrFloor = "ЭТАЖЕЙ";
        }

        /// <summary>
        /// Показ настроек пользователю для просмотра и редактирования.
        /// </summary>        
        public static void PromptOptions()
        {            
            //Запрос начальных значений
            AcadLib.UI.FormProperties formProp = new AcadLib.UI.FormProperties();
            Options newOptions = (Options)_instance.MemberwiseClone();
            formProp.propertyGrid1.SelectedObject = newOptions;
            if (Application.ShowModalDialog(formProp) != System.Windows.Forms.DialogResult.OK)
            {
                throw new Exception(General.CanceledByUser);
            }
            try
            {
                _instance = newOptions;
                _instance.Save();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "Не удалось сохранить стартовые параметры.");
            }            
        }

        static Options Load()
        {
            Options options = null;
            if (File.Exists(FileXml))
            {
                try
                {
                    // Загрузка настроек таблицы из файла XML
                    options = Options.LoadFromXml();
                    // Загрузка начтроек чертежа
                    options.LoadFromNOD();
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, $"Ошибка при попытке загрузки настроек таблицы из XML файла {FileXml}");
                }
            }

            if (options == null)
            {
                // Создать дефолтные
                options = new Options();
                options.SetDefault();
                // Сохранение дефолтных настроек 
                try
                {
                    options.Save();
                }
                catch (Exception exSave)
                {
                    Logger.Log.Error(exSave, $"Попытка сохранение настроек в файл {FileXml}");
                }
            }
            return options;
        }

        void Save()
        {
            SaveToNOD();
            AcadLib.Files.SerializerXml ser = new AcadLib.Files.SerializerXml(FileXml);
            ser.SerializeList(this);
        }

        static Options LoadFromXml()
        {
            AcadLib.Files.SerializerXml ser = new AcadLib.Files.SerializerXml(FileXml);
            return ser.DeserializeXmlFile<Options>();
        }        

        void SaveToNOD()
        {
            //var nod = new AcadLib.DictNOD(DictNod);
            //nod.Save(AbsoluteZero, RecAbsoluteZero);
        }

        void LoadFromNOD()
        {
            //var nod = new DictNOD(DictNod);
            //AbsoluteZero = nod.Load(RecAbsoluteZero, 150.00);
        }
    }
}
