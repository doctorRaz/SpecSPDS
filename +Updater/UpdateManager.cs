using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Src.Services;
using drz.Updater.Services;
using drz.Updater.Services.SevenZip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drz.Updater
{
    /// <summary>
    /// Менеджер обновлений
    /// </summary>
    public class UpdateManager
    {
        private IDrzLogger _logger;//логгер

        IAddOnInfo _addOnInfo { get; }

        public UpdateManager(IAddOnInfo addOnInfo, IMessageService msg, UpdateMode updateMode, bool isManual = false)
        {
            _addOnInfo = addOnInfo;

           // _logger = IDrzLoggerFactory /*LoggerProvider*/.GetLogger<UpdateManager>(addOnInfo.ProductName);

            ICommandLineMessageService msgC = (ICommandLineMessageService)msg;

            msgC.ConsoleMessage(addOnInfo.ProductName);

            IWindowMessageService msgW = (IWindowMessageService)msg;

            msgW.InfoMessage(addOnInfo.PackageDirectory);

        }
        // получаю:
        //	adonInfo, 
        //	message
        //	logger 
        //	флаг обновления: 
        //		не обновлять, 
        //		проверять спрашивать
        //		обновлять молча
        //	флаг как запущен обновлятор: 
        //		руками
        //		программно
        //-----------------
        // запускаем очистку роот
        //-----------------
        // читаем update.settings (лежит в каталоге аддона рядом с обновлятором), если его нет создаем класс в памяти
        // формируем строку uri для скачивания update.json
        //-----------------
        //	если запущено руками идем проверять иначе
        //		если разрешено проверять
        //			проверяем из update.settings что сегодня проверок не было (сделать класс IsСегодня)
        //				скачиваем json
        //		если проверять не разрешено, смотрим прошло ли 7 дней с последней проверки (сделать класс IsСемьдней)
        //			если прошло то скачиваем json
        //-----------------
        // update.json успешно скачан, читаем update.json в память
        // пишем в update.settings дату последнего обновления
        //-----------------
        // update.json:
        //		имя файла обновления
        //		флаг полное обновление (переименовать весь root в bak
        //		флаг обязательное обновление
        //		......
        // удаляем update.json
        //-----------------
        //если обновление новее (проверка по инсталлед версии) то
        //		если обязательное или разрешено без уведомления, скачиваем в темп рандомное имя с проверкой, что такого нет
        // 		если обновление разрешено с уведомлением, уведомляем (запрос обновлять или нет)
        //			если юзер подтвердил скачиваем, скачиваем в темп рандомное имя с проверкой, что такого нет
        //			иначе выход
        //-----------------
        // проверяем скачанный архив на SHA (опционально)
        // распаковываем в темп, в ранодомный каталог с проверкой на отсутствие
        // удаляем скачанный архив
        //-----------------
        // делаем бэкап всего root в темп, рандомное имя (исключаем все *.7z из архива)
        // удаляем все 7z рядом с package (бэкапы)
        //копируем бэкап из темпа рядом с package, имя продукт+признак када+старая версия (опционально батник для распаковки)
        //		если в update.json флаг полное обновление, переименовываем весь root (исключаем *.7z бэкапы) (возможно какие то файлы будут не нужны, удалятся при следующем запуске)
        //-----------------
        // запускаем инсталлер, замену перемещение файлов из темп
        // 	если все прошло хорошо удаляем распакованный архив и сообщаем юзеру, что обновились (ком строка или нотифай)
        //	если плохо месадж бокс, что косяк с обновлением и просьбой закрыть нк и запустить батник (распаковать бэкап на место старого архива)


        /// <summary>Runs the specified add on information.</summary>
        /// <param name="addOnInfo">The add on information.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="updateMode">The update mode.</param>
        /// <param name="isManual">if set to <c>true</c> [is manual].</param>
        /// <returns></returns>
        public static bool Run(IAddOnInfo addOnInfo, IMessageService msg, UpdateMode updateMode, bool isManual=false)
        {
            

        

            return true;
        }

    }
}
