using System.ComponentModel;

namespace dRz.SpecSPDS.Enums
{
    /// <summary>
    /// пространство
    /// </summary>
    public enum Space
    {
        /// <summary> Обрабатывать все блоки в текущем файле </summary>
        [Description("Все")]
        All,
        /// <summary>Обрабатывать все блоки в активном пространстве </summary>
        [Description("Пространство")]
        Layout,
        /// <summary> Обрабатывать выбранные блоки в текущем файле </summary>
        [Description("вЫбрать")]
        Select,

    }
}
