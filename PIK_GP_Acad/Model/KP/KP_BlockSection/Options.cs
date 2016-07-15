﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;

namespace PIK_GP_Acad.KP.KP_BlockSection
{
    [Serializable]
    public class OptionsKPBS
    {
        //static string FileXml = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder,
        //                               @"ГП\KP_BlockSection.xml");        
        const string DictName = "KP_BlockSection";
        const string KeyNormAreaPepPerson = "NormAreaPepPerson";
        const string KeyNormSchoolPlace = "NormSchoolPlace";
        const string KeyNormKinderPlace = "NormKinderPlace";
        const string KeyNormParking = "NormParking";
        const string KeyNormParkingAreaPerPerson = "NormParkingAreaPerPerson";
        const string KeyNormParkingPlaceFor100 = "NormParkingPlaceFor100";
        const string KeyTextStyleItalic = "TextStyleItalic";

        static OptionsKPBS _instance;
        public static OptionsKPBS Instance {
            get {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }

        [Browsable(false)]
        public string LayerBSContourGNS { get; set; } = "ГП_Секции_ГНС";

        [Browsable(false)]
        [Category("Блок-секция")]
        [DisplayName("Имя блока секции")]
        [Description("Соответствие имени блока Блок-Секции. ^ГП_К_Секция - имя блока начинается с ГП_К_Секция. Регистр игнорируется.")]
        [DefaultValue("^ГП_К_Секция")]
        public string BlockSectionNameMatch { get; set; } = "^ГП_К_Секция";

        [Browsable(false)]
        [Category("Блок-секция")]
        [DisplayName("Атрибут этажности")]
        [Description("Тег атрибута Этажности в блоке Блок-секции")]
        [DefaultValue("ЭТАЖ")]
        public string BlockSectionAtrFloor { get; set; } = "ЭТАЖ";

        private int _normAreaPerPerson =30;
        /// <summary>
        /// Обеспеченность
        /// </summary>
        [Category("Нормативные показатели")]
        [DisplayName("Обеспеченность")]
        [DefaultValue(30)]
        [Description("Количество м2 ж.ф. на человека.\nЖители = <ЖФ типовых этажей> / <Обеспеченность>.")]
        public int NormAreaPerPerson {
            get { return _normAreaPerPerson; }
            set {
                if (value > 0)
                    _normAreaPerPerson = value;                
            }
        }

        private int _normSchoolPlace = 124;
        /// <summary>
        /// Норма мест СОШ на 1000
        /// </summary>
        [Category("Нормативные показатели")]
        [DisplayName("Мест СОШ на 1000чел")]
        [DefaultValue(124)]
        [Description("Количество мест СОШ на 1000 жителей")]
        public int NormSchoolPlace {
            get { return _normSchoolPlace; }
            set {
                if (value > 0)
                    _normSchoolPlace = value;
            }
        }

        private int _normKinderPlace = 54;
        /// <summary>
        /// Норма мест ДОО на 1000
        /// </summary>
        [Category("Нормативные показатели")]
        [DisplayName("Мест ДОО на 1000чел")]
        [DefaultValue(54)]
        [Description("Количество мест ДОО на 1000 жителей")]
        public int NormKinderPlace {
            get { return _normKinderPlace; }
            set {
                if (value > 0)
                    _normKinderPlace = value;
            }
        }
        
        /// <summary>
        /// М/м на 1000
        /// </summary>
        [Category("Нормы парковки")]
        [DisplayName("М/м на 1000чел")]
        [DefaultValue(350)]
        [Description("Количество м/м на 1000 жителей при расчете постоянного паркинга.\nПостоянный паркинг = <Жители> * <М/м на 1000чел> * 0.9 / 1000")]
        [TypeConverter(typeof(NormParkingConverter))]
        public int NormParking { get; set; } = 350;

        private int _normAreaBKFNPerPerson = 20;
        /// <summary>
        /// Площадь парковки на человека
        /// </summary>
        [Category("Нормы парковки")]
        [DisplayName("Площадь БКФН на человека")]
        [DefaultValue(20)]
        [Description("Кол м2 БКФН на одного человека.")]
        public int NormAreaBKFNPerPerson {
            get { return _normAreaBKFNPerPerson; }
            set {
                if (value > 0)
                    _normAreaBKFNPerPerson = value;
            }
        }

        private int _normParkingPlaceFor100 =5;
        /// <summary>
        /// Площадь парковки на человека
        /// </summary>
        [Category("Нормы парковки")]
        [DisplayName("М/м на 100чел")]
        [DefaultValue(5)]
        [Description("Кол м/м на 100 человек при расчете паркинга БКФН.")]
        public int NormParkingPlaceFor100 {
            get { return _normParkingPlaceFor100; }
            set {
                if (value > 0)
                    _normParkingPlaceFor100 = value;
            }
        }

        /// <summary>
        /// Курсив
        /// </summary>
        [Category("Оформление")]
        [DisplayName("Курсив")]
        [DefaultValue(false)]
        [Description("Курсивный шрифт в таблице.")]
        [Editor(typeof(CheckBoxEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(BooleanTypeConverter))]
        public bool TextStyleItalic { get; set; } = false;

        public OptionsKPBS()
        {
        }        

        void SetDefault()
        {
            
        }

        /// <summary>
        /// Показ настроек пользователю для просмотра и редактирования.
        /// </summary>        
        public static void PromptOptions()
        {            
            //Запрос начальных значений
            AcadLib.UI.FormProperties formProp = new AcadLib.UI.FormProperties();
            OptionsKPBS newOptions = (OptionsKPBS)Instance.MemberwiseClone();
            formProp.propertyGrid1.SelectedObject = newOptions;
            if (Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(formProp) != System.Windows.Forms.DialogResult.OK)
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

        static OptionsKPBS Load()
        {
            OptionsKPBS options = new OptionsKPBS();
            options.LoadFromNOD();
            //Options options = null;
            //if (File.Exists(FileXml))
            //{
            //try
            //{
            //// Загрузка настроек таблицы из файла XML
            //options = Options.LoadFromXml();
            // Загрузка начтроек чертежа
            //options.LoadFromNOD();
            //}
            //catch (Exception ex)
            //{
            //    Logger.Log.Error(ex, $"Ошибка при попытке загрузки настроек таблицы из XML файла {FileXml}");
            //}
            //}

            //if (options == null)
            //{
            //    // Создать дефолтные
            //    options = new Options();
            //    options.SetDefault();
            //    // Сохранение дефолтных настроек 
            //    try
            //    {
            //        options.Save();
            //    }
            //    catch (Exception exSave)
            //    {
            //        Logger.Log.Error(exSave, $"Попытка сохранение настроек в файл {FileXml}");
            //    }
            //}
            return options;
        }

        void Save()
        {
            SaveToNOD();
            //AcadLib.Files.SerializerXml ser = new AcadLib.Files.SerializerXml(FileXml);
            //ser.SerializeList(this);
        }

        //static Options LoadFromXml()
        //{
        //    AcadLib.Files.SerializerXml ser = new AcadLib.Files.SerializerXml(FileXml);
        //    return ser.DeserializeXmlFile<Options>();
        //}        

        void SaveToNOD()
        {
            var nod = new DictNOD(DictName, true);
            nod.Save(NormAreaPerPerson, KeyNormAreaPepPerson);
            nod.Save(NormSchoolPlace, KeyNormSchoolPlace);
            nod.Save(NormKinderPlace, KeyNormKinderPlace);
            nod.Save(NormParking, KeyNormParking);
            nod.Save(NormAreaBKFNPerPerson, KeyNormParkingAreaPerPerson);
            nod.Save(NormParkingPlaceFor100, KeyNormParkingPlaceFor100);
            nod.Save(TextStyleItalic, KeyTextStyleItalic);
        }

        void LoadFromNOD()
        {
            var nod = new DictNOD(DictName, true);
            NormAreaPerPerson = nod.Load(KeyNormAreaPepPerson, 30);
            NormSchoolPlace = nod.Load(KeyNormSchoolPlace, 124);
            NormKinderPlace = nod.Load(KeyNormKinderPlace, 54);
            NormParking = nod.Load(KeyNormParking, 350);
            NormAreaBKFNPerPerson = nod.Load(KeyNormParkingAreaPerPerson, 20);
            NormParkingPlaceFor100 = nod.Load(KeyNormParkingPlaceFor100, 5);
            TextStyleItalic = nod.Load(KeyTextStyleItalic, false);
        }
    }

    class NormParkingConverter : TypeConverter
    {
        private List<int> values = new List<int> () { 420,350 };
        public override bool GetStandardValuesSupported (ITypeDescriptorContext context) { return true; }

        public override bool GetStandardValuesExclusive (ITypeDescriptorContext context) { return true; }

        public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(values);
        }
        public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                return Convert.ToInt32(value);                
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class BooleanTypeConverter : BooleanConverter
    {
        public override object ConvertFrom (ITypeDescriptorContext context,
          CultureInfo culture,
          object value)
        {
            return (string)value == "Да";
        }

        public override object ConvertTo (ITypeDescriptorContext context,
                CultureInfo culture,
          object value,
          Type destType)
        {
            return (bool)value ?
              "Да" : "Нет";
        }
    }

    public class CheckBoxEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override bool GetPaintValueSupported (ITypeDescriptorContext context)
        {
            return true;
        }                     
        

        public override void PaintValue (PaintValueEventArgs e)
        {            
            ButtonState State;
            bool res=Convert.ToBoolean((e.Value));
            if (res)
            {
                State =ButtonState.Checked;                
            }
            else
            {
                State = ButtonState.Normal;
            }
            
            ControlPaint.DrawCheckBox(e.Graphics, e.Bounds, State);                        
            e.Graphics.ExcludeClip(e.Bounds);            
        }

        public override object EditValue (ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            return base.EditValue(context, provider, "Курсив");
        }
    }
}