using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Настройки инсоляции для пользователя.
    /// Сохраняется у пользователя на компьютере.
    /// </summary>
    public class UserSettings : ModelBase
    {
        private static string file = AcadLib.IO.Path.GetUserPluginFile(InsService.PluginName, "InsUserSettings.xml");

        public UserSettings()
        {

        }

        public bool EnableCheckDublicates { get { return enableCheckDublicates; }
            set { enableCheckDublicates = value; RaisePropertyChanged();} }
        bool enableCheckDublicates;

        public static UserSettings Load ()
        {
            
            var userSettings = AcadLib.Files.SerializerXml.Load<UserSettings>(file);
            if (userSettings == null)
            {
                userSettings = GetDefault();
            }
            return userSettings;
        }

        public void Save()
        {
            AcadLib.Files.SerializerXml.Save(file, this);
        }

        private static UserSettings GetDefault()
        {
            return new UserSettings
            {
                EnableCheckDublicates = true
            };
        }

        public UserSettings Copy()
        {
            return (UserSettings)MemberwiseClone();
        }
    }
}
