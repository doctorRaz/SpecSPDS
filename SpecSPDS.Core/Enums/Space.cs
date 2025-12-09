using System.ComponentModel;

namespace dRz.SpecSPDS.Core.Enums
{
    /// <summary>
    /// пространство
    /// </summary>
    public enum Space
    {

        /// <summary> Обрабатывать все маркеры в открытых файлах </summary>
        [Description("Все")]
        All,
        /// <summary> Обрабатывать все маркеры в текущем файле </summary>
        [Description("Документ")]
        Document,
        /// <summary>Обрабатывать все маркеры в активном пространстве </summary>
        [Description("Текущее")]
        Layout,
        /// <summary> Обрабатывать выбранные маркеры в текущем файле </summary>
        [Description("вЫбрать")]
        Select,

    }
}
