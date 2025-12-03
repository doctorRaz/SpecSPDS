using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dRz.SpecSPDS.Core.Settings
{
    public class ApplicationSettings
    {
        public string ApplicationName { get; set; } = "SpecSPDS";

        /// <summary> Имя маркера, можно только часть имени, регистр неважен </summary>
        public string MarkerName { get; set; } = "Спецификация";

        /// <summary>
        /// Названия полей маркера
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public FieldNameSettings FieldNames{ get; set; }=new FieldNameSettings();
    }
}
