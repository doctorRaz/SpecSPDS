using dRz.SpecSPDS.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace dRz.SpecSPDS.Core.Services
{
    public class PropXml
    {

        public PropXml()
        {
            //LoadProps();
        }

        public List< DefinitionMarkerProps> Props
        {
            get => _props;
            set => _props = value ?? new List< DefinitionMarkerProps>();
        }

        public void LoadProps()

        {
            try
            {
                if (File.Exists(_configPath))
                {
                    using (FileStream stream = new FileStream(_configPath, FileMode.Open))
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

        public void SaveProps()
        {
            string folder = Path.GetDirectoryName(_configPath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (FileStream writer = new FileStream(_configPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                _serializer.Serialize(writer, _props);
            }


        }
        private /*static*/ List< DefinitionMarkerProps> _props;

        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(List<DefinitionMarkerProps>));

        private static readonly string _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                          "SpecSPDS",
                                                          "prop.xml");
    }
}
