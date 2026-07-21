using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Updater.Services;

namespace drz.Updater
{
    /// <summary>
    /// Менеджер обновлений
    /// </summary>
    public class UpdateManager
    {
        private readonly IDrzLogger _logger;//логгер
        private readonly IMessageService _messageServices;
        private readonly IAddOnInfo _addOnInfo;

        /// <summary>Initializes a new instance of the <see cref="UpdateManager"/> class.</summary>
        /// <param name="addOnInfo">The add on information.</param>
        /// <param name="messageServices">The message services.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="updateMode">The update mode.</param>
        /// <param name="isManual">if set to <c>true</c> [is manual].</param>
        public UpdateManager(IAddOnInfo addOnInfo, IMessageService messageServices, IDrzLoggerFactory loggerFactory, UpdateMode updateMode, bool isManual = false)
        {
            _addOnInfo = addOnInfo;
            _messageServices = messageServices;
            _logger = loggerFactory.GetLogger<UpdateManager>();

            _logger.Debug("Updater");

            _messageServices.ConsoleMessage(addOnInfo.ProductName);

            Run();

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


        /// <summary>Runs this instance.</summary>
        /// <returns></returns>
        public bool Run()
        //public   bool Run(IAddOnInfo addOnInfo, IMessageService msg, IDrzLoggerFactory drzLoggerFactory, UpdateMode updateMode, bool isManual = false)
        {



            _logger.Debug("Run");
            _logger.DebugCaller("Архив создан");
            _messageServices.ConsoleMessage(_addOnInfo.ProductName+".Run");
            return true;
        }

    }
}
