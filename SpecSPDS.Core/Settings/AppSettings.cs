using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace dRz.SpecSPDS.Core.Settings
{
    public class AppSettings
    {
        /*static*/
       public AppSettings()
        {
            LoadSettings();


        }

        public ApplicationSettings Settings
        {
            get => _settings;
            set => _settings = value ?? new ApplicationSettings();
        }

        public  ApplicationSettings DefaultSettings
        {
            get => new ApplicationSettings();
        }

        public void SaveSettings()
        {
            string folder = Path.GetDirectoryName(_configPath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (FileStream writer = new FileStream(_configPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                _serializer.Serialize(writer, _settings);
            }


        }
        public void LoadSettings()

        {
            try
            {
                if (File.Exists(_configPath))
                {
                    using (FileStream stream = new FileStream(_configPath, FileMode.Open))
                    {
                        _settings = (ApplicationSettings)_serializer.Deserialize(stream);
                    }
                }
                else
                {
                    _settings = new ApplicationSettings();
                }
            }
            catch
            {
                _settings = new ApplicationSettings();
            }
        }

        public string ConfigPath => _configPath;

        private static ApplicationSettings _settings;

        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(ApplicationSettings));

        private static readonly string _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                                  "SpecSPDS",
                                                                  "SpecSPDS.config");
    }
}
