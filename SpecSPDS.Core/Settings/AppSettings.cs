using drz.SpecSPDS.Core.Settings;
using System;
using System.IO;
using System.Xml.Serialization;

namespace dRz.SpecSPDS.Core.Settings
{

    public sealed class AppSettings
    {
        private static readonly XmlSerializer _serializer =
            new XmlSerializer(typeof(ApplicationSettings));

        private ApplicationSettings _settings;

        public ApplicationSettings Settings =>
            _settings ??= new ApplicationSettings();

        public string ConfigPath => AppPaths.ConfigFile;

        public void Load()
        {
            if (!File.Exists(ConfigPath))
            {
                _settings = new ApplicationSettings();
                Save();
                return;
            }

            using var stream = File.OpenRead(ConfigPath);
            _settings = (ApplicationSettings)_serializer.Deserialize(stream);
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);

            using var stream = File.Create(ConfigPath);
            _serializer.Serialize(stream, Settings);
        }
    }


    //public class AppSettings
    //{
    //    /*static*/
    //    public AppSettings()
    //    {
    //        LoadSettings();


    //    }

    //    public ApplicationSettings Settings
    //    {
    //        get => _settings;
    //        set => _settings = value ?? new ApplicationSettings();
    //    }

    //    public ApplicationSettings DefaultSettings
    //    {
    //        get => new ApplicationSettings();
    //    }

    //    public void SaveSettings()
    //    {
    //        string folder = Path.GetDirectoryName(_configPath);
    //        if (!Directory.Exists(folder))
    //        {
    //            Directory.CreateDirectory(folder);
    //        }

    //        using (FileStream writer = new FileStream(_configPath, FileMode.Create, FileAccess.Write, FileShare.None))
    //        {
    //            _serializer.Serialize(writer, _settings);
    //        }


    //    }
    //    public void LoadSettings()

    //    {
    //        try
    //        {
    //            if (File.Exists(_configPath))
    //            {
    //                using (FileStream stream = new FileStream(_configPath, FileMode.Open))
    //                {
    //                    _settings = (ApplicationSettings)_serializer.Deserialize(stream);
    //                }
    //            }
    //            else
    //            {
    //                _settings = new ApplicationSettings();
    //            }

    //            SaveSettings();//если добавилось свойство
    //        }
    //        catch
    //        {
    //            _settings = new ApplicationSettings();
    //        }
    //    }

    //    public string ConfigPath => _configPath;

    //    private /*static*/ ApplicationSettings _settings;

    //    private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(ApplicationSettings));

    //    private static readonly string _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    //                                                              "SpecSPDS",
    //                                                              "SpecSPDS.config");
    //}


}
