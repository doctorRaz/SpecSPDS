using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace dRz.SpecSPDS.Core.Settings
{
    public class ApplicationSettings
    {
        public string ApplicationName { get; set; } = "SpecSPDS";

        /// <summary> Имя маркера, можно только часть имени, регистр неважен </summary>
        public string MarkerName { get; set; } = "Спецификация";

        /// <summary>
        /// Собирать маркеры с флагом спецификации
        /// </summary>
        /// <value>
        ///   <c>true</c> Собирать только с флагом true (1); otherwise (0), <c>false</c>.
        /// </value>
        public bool IsSpec{ get; set; }=true;

        /// <summary>
        /// Названия полей маркера
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public FieldNameSettings FieldNames{ get; set; }=new FieldNameSettings();
    }
}
