namespace dRz.SpecSPDS.Core.Settings
{
    public class ApplicationSettings
    {
        public string ApplicationName { get; set; } = "SpecSPDS";

        /// <summary>
        /// Gets or sets the unique identifier. McUmarker
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public string Guid { get; set; } = "00000001-0014-aaaa-aaaa-050b00000000";//McUmarker

        /// <summary> Имя маркера, можно только часть имени, регистр неважен </summary>
        public string MarkerName { get; set; } = "Спецификация";

        /// <summary>
        /// Собирать маркеры с флагом спецификации
        /// </summary>
        /// <value>
        ///   <c>true</c> Собирать только с флагом true (1); otherwise (0), <c>false</c>.
        /// </value>
        public bool IsSpec { get; set; } = true;

        /// <summary>
        /// Названия полей маркера
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public FieldNameSettings FieldNames { get; set; } = new FieldNameSettings();
    }
}
