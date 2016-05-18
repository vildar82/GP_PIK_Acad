using System;
using System.IO;
using System.Reflection;
using AcadLib.Files;

namespace PIK_GP_Acad.BlockSection
{
    [Serializable]
    public class Settings
    {
        private static string _curDllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static Settings _settings;

        public static Settings Default
        {
            get
            {
                if (_settings == null) _settings = Load();
                return _settings;
            }
        }

        public static string FileSettings { get { return Path.Combine(_curDllDir, "GP_BlockSection.xml"); } }
        public string AttrAreaApart { get; set; }
        public string AttrAreaApartTotal { get; set; }
        public string AttrAreaBKFN { get; set; }
        public string AttrName { get; set; }
        public string AttrNumberFloor { get; set; }
        public string BlockSectionPrefix { get; set; }

        private static Settings Load()
        {
            Settings res;
            if (File.Exists(FileSettings))
            {
                try
                {
                    SerializerXml ser = new SerializerXml(FileSettings);
                    res = ser.DeserializeXmlFile<Settings>();
                }
                catch
                {
                    res = new Settings();
                    res.SetDefault();
                }
            }
            else
            {
                res = new Settings();
                res.SetDefault();
                res.Save();
            }
            return res;
        }

        private void Save()
        {
            try
            {
                SerializerXml ser = new SerializerXml(FileSettings);
                ser.SerializeList(this);
            }
            catch
            {
            }
        }

        private void SetDefault()
        {
            BlockSectionPrefix = "ГП_Блок-секция";
            AttrName = "НАИМЕНОВАНИЕ";
            AttrAreaBKFN = "БКФН";
            AttrAreaApart = "КВЭТ";
            AttrAreaApartTotal = "КВОБЩ";
            AttrNumberFloor = "ЭТАЖЕЙ";
        }
    }
}