namespace dRz.SpecSPDS.Core.Settings
{
    /// <summary>
    /// имена полей свойств маркера
    /// </summary>
    public class FieldNameSettings
    {
        public FieldNameSettings()
        {
        }



        /// <summary>
        /// Раздел спецификации
        /// </summary>
        public string Section { get; set; } = "razdel";

        /// <summary>
        /// Позиция
        /// </summary>
        public string PositionNumber { get; set; } = "POZS";

        /// <summary>
        /// Наименование и техническая характеристика
        /// </summary>
        public string DeviceName { get; set; } = "naim";

        /// <summary>
        /// Тип, марка, обозначение документа, опросного листа
        /// </summary>
        public string TypeModel { get; set; } = "tip";

        /// <summary>
        /// Код продукции
        /// </summary>
        public string ArticleNumber { get; set; } = "kod";

        /// <summary>
        /// Поставщик
        /// </summary>
        public string Vendor { get; set; } = "zavod";

        /// <summary>
        /// Единица измерения
        /// </summary>
        public string Unit { get; set; } = "ed";

        /// <summary>
        /// Количество
        /// </summary>
        public string Amount { get; set; } = "kolrez";

        /// <summary>
        /// Масса 1 ед.кг
        /// </summary>
        public string UnitMass { get; set; } = "";

        /// <summary>
        /// Примечание
        /// </summary>
        public string Comment { get; set; } = "prim";

        /// <summary>
        /// Флаг включения в спецификацию 
        /// </summary>         
        public string FlagSpec { get; set; } = "flag";
    }
}
