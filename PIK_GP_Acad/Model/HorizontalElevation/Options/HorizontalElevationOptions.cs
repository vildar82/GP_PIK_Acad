using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AcadLib;
using AcadLib.Files;
using Autodesk.AutoCAD.ApplicationServices;

namespace PIK_GP_Acad.Model.HorizontalElevation
{
   /// <summary>
   /// Настройки для горизронталей
   /// </summary>
   [Serializable]
   public class HorizontalElevationOptions
   {
      private static readonly string fileOptions = Path.Combine(
                     AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder,
                     "ГП\\GP-HorizontalElevation.xml");
      private static HorizontalElevationOptions _instance;
      public static HorizontalElevationOptions Instance
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

      private HorizontalElevationOptions() { }

      /// <summary>
      /// Начальный уровень 
      /// </summary>      
      [Description("Начальный уровень для горизонталей поумолчанию.")]
      //[DefaultValue(100)]
      [XmlElement("StartElevation")]
      public double StartElevation { get; set; } = 100;

      /// <summary>
      /// Шаг приращения
      /// </summary>      
      [Description("Шаг приращения уровня горизонталей поумолчанию.")]
      //[DefaultValue(10)]
      [XmlElement("StepElevation")]
      public double StepElevation { get; set; } = 10;

      ///// <summary>
      ///// Цвет текста
      ///// </summary>      
      //[Description("Цвет текста.")]
      //[DefaultValue(null)]
      //[XmlIgnore]
      //[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      //public Color TextColor { get; set; } = Color.Aqua;

      //[XmlElement("TextColor")]
      //[Browsable(false)]
      //public int TextColorAsArgb
      //{
      //   get { return TextColor.ToArgb(); }
      //   set { TextColor = Color.FromArgb(value); }
      //}


      /// <summary>
      /// Высота текста - относительно высоте текущего вида
      /// </summary>      
      [Description("Высота текста - относительно высоты текущего вида.")]
      //[DefaultValue(0.02)]      
      [XmlElement("TextHeight")]
      public double TextHeight { get; set; } = 0.02;

      public static HorizontalElevationOptions Load()
      {
         HorizontalElevationOptions options = null;
         // загрузка из файла настроек
         if (File.Exists(fileOptions))
         {
            SerializerXml xmlSer = new SerializerXml(fileOptions);
            try
            {
               options = xmlSer.DeserializeXmlFile<HorizontalElevationOptions>();
               if (options != null)
               {
                  return options;
               }
            }
            catch (Exception ex)
            {
               Logger.Log.Error(ex, $"Не удалось десериализовать настройки из файла {fileOptions}");
            }
         }
         options = new HorizontalElevationOptions();
         options.Save();
         return options;
      }

      public void Save()
      {
         try
         {
            if (!File.Exists(fileOptions))
            {
               Directory.CreateDirectory(Path.GetDirectoryName(fileOptions));
            }
            SerializerXml xmlSer = new SerializerXml(fileOptions);
            xmlSer.SerializeList(this);
         }
         catch (Exception ex)
         {
            Logger.Log.Error(ex, $"Не удалось сериализовать настройки в {fileOptions}");
         }
      }      

      public static void Show()
      {
         FormOptions formOpt = new FormOptions((HorizontalElevationOptions)Instance.MemberwiseClone());
         if (Application.ShowModalDialog(formOpt) == System.Windows.Forms.DialogResult.OK)
         {
            _instance = formOpt.Options;
            _instance.Save();
         }
      }
   }
}
