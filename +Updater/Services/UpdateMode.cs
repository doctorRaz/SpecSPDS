using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drz.Updater.Services
{
    /// <summary>
    /// Режим проверки обновлений
    /// </summary>
    public enum UpdateMode
    {
        /// <summary>
        /// не проверять
        /// </summary>
        Disabled,
        /// <summary>
        /// проверить и уведомить
        /// </summary>
        CheckAndNotify,
        /// <summary>
        /// проверить и установить
        /// </summary>
        CheckAndInstall
    }
}
