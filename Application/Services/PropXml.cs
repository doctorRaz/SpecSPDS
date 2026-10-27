using dRz.Application.Models;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace dRz.Application.Services
{
    /// <summary>
    /// Загружает и сохраняет свойства маркеров спецификации в XML-файле.
    /// </summary>
    public class PropXml
    {
        /// <summary>
        /// Создаёт объект работы с XML-свойствами.
        /// </summary>
        public PropXml()
        {
            //LoadProps();
        }

        /// <summary>
        /// Список свойств маркеров спецификации.
        /// </summary>
        public List<DefinitionMarkerProps> Props
        {
            get => _props;
            set => _props = value ?? new List<DefinitionMarkerProps>();
        }

        /// <summary>
        /// Загружает свойства из XML-файла; при отсутствии файла или ошибке создаёт пустой список.
        /// </summary>
        public void LoadProps()

        {
            try
            {
                if (File.Exists(_xmlPath))
                {
                    using (FileStream stream = new FileStream(_xmlPath, FileMode.Open))
                    {
                        _props = (List<DefinitionMarkerProps>)_serializer.Deserialize(stream);
                    }
                }
                else
                {
                    _props = new List<DefinitionMarkerProps>();
                }
            }
            catch
            {
                _props = new List<DefinitionMarkerProps>();
            }
        }

        /// <summary>
        /// Сохраняет свойства маркеров в XML-файл.
        /// </summary>
        public void SaveProps()
        {
            string folder = Path.GetDirectoryName(_xmlPath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (FileStream writer = new FileStream(_xmlPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                _serializer.Serialize(writer, _props);
            }
        }

        private List<DefinitionMarkerProps> _props;

        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(List<DefinitionMarkerProps>));

        //private static readonly string _xmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        //                                                  "SpecSPDS",
        //                                                  "prop.xml");

        private string _xmlPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "prop.xml");
    }
}