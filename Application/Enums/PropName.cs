using System.ComponentModel;

namespace dRz.Application.Enums
{
    /// <summary>
    /// Имена свойств маркера спецификации.
    /// </summary>
    public enum PropName
    {
        /// <summary>
        /// Имя маркера.
        /// </summary>
        [Description("Имя маркера")]
        MarkerName,

        /// <summary>
        /// Раздел.
        /// </summary>
        [Description("Раздел")]
        Section,

        /// <summary>
        /// Номер позиции.
        /// </summary>
        [Description("Номер позиции")]
        PositionNumber,

        /// <summary>
        /// Наименование и техническая характеристика.
        /// </summary>
        [Description("Наименование и техническая характеристика")]
        DeviceName,

        /// <summary>
        /// Тип, марка,обозначение документа,опросного листа.
        /// </summary>
        [Description("Тип, марка,обозначение документа,опросного листа")]
        TypeModel,

        /// <summary>
        /// Код продукции.
        /// </summary>
        [Description("Код продукции")]
        ArticleNumber,

        /// <summary>
        /// Поставщик.
        /// </summary>
        [Description("Поставщик")]
        Vendor,

        /// <summary>
        /// Единица измерения.
        /// </summary>
        [Description("Единица измерения")]
        Unit,

        /// <summary>
        /// Количество.
        /// </summary>
        [Description("Количество")]
        Amount,

        /// <summary>
        /// Масса единицы.
        /// </summary>
        [Description("Масса единицы")]
        UnitMass,

        /// <summary>
        /// Примечание.
        /// </summary>
        [Description("Примечание")]
        Comment,

        /// <summary>
        /// Флаг включения в спецификацию.
        /// </summary>
        [Description("Флаг включения в спецификацию")]
        FlagSpecs,
    }
}