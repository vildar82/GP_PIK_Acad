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

namespace PIK_GP_Acad.Parking
{
    [Serializable]
    public class LineParkingOptions
    {        
        static string FileXml = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder,
                                @"ГП\\Parking\\LineParking.xml");
        const string DictNod = "PIK";
        //public const string RecAbsoluteZero = "AbsoluteZero";

        [Category("Общие")]
        [DisplayName("Имя блока линейной паркови")]
        [Description("Имя блока линии парковки.")]
        [DefaultValue("ГП_Линия-Парковки")]
        public string BlockNameLineParkingMatch { get; set; } = "ГП_Линия-Парковки";

        public LineParkingOptions PromptOptions()
        {
            LineParkingOptions resVal = this;
            //Запрос начальных значений
            AcadLib.UI.FormProperties formProp = new AcadLib.UI.FormProperties();
            LineParkingOptions thisCopy = (LineParkingOptions)resVal.MemberwiseClone();
            formProp.propertyGrid1.SelectedObject = thisCopy;
            if (Application.ShowModalDialog(formProp) != System.Windows.Forms.DialogResult.OK)
            {
                throw new Exception(General.CanceledByUser);
            }
            try
            {
                resVal = thisCopy;
                Save();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "Не удалось сохранить стартовые параметры.");
            }
            return resVal;
        }

        public static LineParkingOptions Load()
        {
            LineParkingOptions options = null;
            if (File.Exists(FileXml))
            {
                try
                {
                    // Загрузка настроек таблицы из файла XML
                    options = LineParkingOptions.LoadFromXml();
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
                options = new LineParkingOptions();
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

        private void SetDefault()
        {            
        }

        private static LineParkingOptions LoadFromXml()
        {
            AcadLib.Files.SerializerXml ser = new AcadLib.Files.SerializerXml(FileXml);
            return ser.DeserializeXmlFile<LineParkingOptions>();
        }

        public void Save()
        {
            SaveToNOD();
            AcadLib.Files.SerializerXml ser = new AcadLib.Files.SerializerXml(FileXml);
            ser.SerializeList(this);
        }

        private void SaveToNOD()
        {
            //var nod = new AcadLib.DictNOD(DictNod);
            //nod.Save(AbsoluteZero, RecAbsoluteZero);
        }

        private void LoadFromNOD()
        {
            //var nod = new AcadLib.DictNOD(DictNod);
            //AbsoluteZero = nod.Load(RecAbsoluteZero, 150.00);
        }
    }
}
