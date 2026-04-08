using System.ComponentModel;

namespace drz.SpecSPDS.Core.Enums
{
    public enum PropName
    {

        [Description("Имя маркера")]
        MarkerName,

        [Description("Раздел")]
        Section,

        [Description("Номер позиции")]
        PositionNumber,

        [Description("Наименование и техническая характеристика")]
        DeviceName,

        [Description("Тип, марка,обозначение документа,опросного листа")]
        TypeModel,

        [Description("Код продукции")]
        ArticleNumber,

        [Description("Поставщик")]
        Vendor,

        [Description("Единица измерения")]
        Unit,

        [Description("Количество")]
        Amount,

        [Description("Масса единицы")]
        UnitMass,

        [Description("Примечание")]
        Comment,

        [Description("Флаг включения в спецификацию")]
        FlagSpecs,

    }
}
