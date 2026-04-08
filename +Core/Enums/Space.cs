using System.ComponentModel;

namespace drz.SpecSPDS.Core.Enums
{
    /// <summary>
    /// пространство
    /// </summary>
    public enum Space
    {

        /// <summary> Обрабатывать все маркеры из файлов </summary>
        [Description("Файлы")]
        Files,

        /// <summary> Обрабатывать все маркеры в каталогах </summary>
        [Description("Подпапки")]
        SubFolder,

        /// <summary> Обрабатывать все маркеры в каталогах </summary>
        [Description("Каталог")]
        Folder,

        /// <summary> Обрабатывать все маркеры в открытых файлах </summary>
        [Description("Открытые")]
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
