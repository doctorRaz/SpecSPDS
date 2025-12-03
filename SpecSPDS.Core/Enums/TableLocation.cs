using System.ComponentModel;

namespace dRz.SpecSPDS.Core.Enums
{
    public enum TableLocation
    {

        /// <summary>Использовать шаблон из базы, из файла, создать на лету</summary>
        [Description("База")]
        Base,
        /// <summary> Использовать шаблоном подходящую таблицу на чертеже </summary>
        [Description("Выбор")]
        Select,

    }
}
