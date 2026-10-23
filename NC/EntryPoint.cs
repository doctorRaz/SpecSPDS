//using static dRz.Src.Infrastructure.AddOnContext;// на класс, чтобы не писать каждый раз длинный путь

// -----------------
using dRz.Abstractions.Infrastructure;
using dRz.Abstractions.Logger;
using dRz.Abstractions.Services;
using dRz.Abstractions.Services.Message;
using dRz.Abstractions.Settings;
using dRz.AddOnRuntime;
using dRz.NC.Infrastructure;
using dRz.Updater.Services;
using System;

#if !TEST

using dRz.NC.Commands;

#endif

#if NC

using App = HostMgd.ApplicationServices;
using HostMgd.EditorInput;

using Rtm = Teigha.Runtime;

#elif AC

using Autodesk.AutoCAD.DatabaseServices;
using App = Autodesk.AutoCAD.ApplicationServices;
using Cad = Autodesk.AutoCAD.ApplicationServices.Application;
using Db = Autodesk.AutoCAD.DatabaseServices;
using Gem = Autodesk.AutoCAD.Geometry;
using Ed = Autodesk.AutoCAD.EditorInput;
using Rtm = Autodesk.AutoCAD.Runtime;

#endif

namespace dRz.NC
{
    /// <summary>
    /// Точка входа аддона для инициализации и завершения работы в CAD-хосте.
    /// </summary>
    public class EntryPoint
#if !TEST
        : Rtm.IExtensionApplication
#endif
    {
        private static bool _isAddOnCompositionRoot;//контейнер наполнен

        private IAddonSettings _settings;// Настройки аддона, полученные из контейнера сервисов.
        private bool _isSettingsProvider;// Настройки аддона успешно получены.

        private IDrzLogger _logger;// Логгер для записи сообщений в журнал.
        private bool _isLoggerProvider;// Логгер успешно получен.

        private IDrzLoggerFactory _loggerFactory;// Фабрика логгеров для создания логгеров в других классах.
        private bool _isLoggerFactoryProvider;// Фабрика логгеров успешно получена.

        private IMessageService _message;// Сервис пользовательских сообщений.
        private bool _isMessageProvider;// Сервис пользовательских сообщений получен.

        private IWindowMessageService _windowMessage;// Сервис оконных сообщений.
        private bool _isWindowMessageProvider;// Сервис оконных сообщений получен.

        private IMessagePromptService _messagePrompt;// Сервис пользовательских сообщений с запросом.
        private bool _isMessagePromptProvider;// Сервис пользовательских сообщений с запросом получен.

        private IAddOnInfo _addOnInfo;// Сведения о сборке аддона.
        private bool _isAddOnInfoProvider; // Сведения о сборке аддона успешно получены.

        private ICadInfo _cadInfo;// Сведения о CAD-хосте.
        private bool _isCadInfoProvider;// Сведения о CAD-хосте успешно получены.

        private ISysInfo _sysInfo;// Сведения о системе.
        private bool _isSysInfoProvider;//  Сведения о системе успешно получены.

        /// <summary>
        /// Инициализирует аддон: регистрирует сервисы и настройки, запускает проверку обновлений
        /// и выполняет оставшуюся инициализацию.
        /// </summary>
        /// <remarks>
        /// Метод вызывается CAD-хостом через <see cref="Rtm.IExtensionApplication"/>.
        /// Ошибки инициализации перехватываются здесь, чтобы не передавать исключение хосту.
        /// </remarks>
        public void Initialize()
        {
            try
            {
                // Инициализация сервисов аддона и получение необходимых сервисов из контейнера.
                InitializeServices();

                // Проверяем, что аддон загружен в родной каталог, и предупреждаем пользователя о возможных ошибках.

                if (!_addOnInfo.IsProductDirectory && _isWindowMessageProvider)
                {
                    _windowMessage.WarningMessage(
                        $"Приложение не в своем родном каталоге" +
                        $"\n{_addOnInfo.PackageDirectory}" +
                        $"\nили не найден {_addOnInfo.Product}*" +
                        $".package" +
                        $"\n\nВозможны ошибки в работе и при обновлении!");
                }

                // Запускаем асинхронное обновление и очистку служебных файлов в фоновом процессе.
                StartUpdate();

                // Записываем сведения о сборке аддона и CAD-хосте в журнал и выводим приветственное сообщение.
                ReportInitialization();
            }
            catch (Exception ex) // Ошибка общей инициализации: сообщаем пользователю и записываем её в журнал, если сервисы доступны.
                                 // Если ошибка поднялась сюда, то вероятно, что аддон неработоспособен
            {
                string msg = $"Приложение не загружено!!!";

                if (_isLoggerProvider)// Используем логгер только после успешного получения сервиса.
                {
                    msg += $"\nОтправьте разработчику логфайлы";

                    if (_isAddOnInfoProvider)
                    {
                        msg += $": {_addOnInfo.ProductDataDirectory}\\logs";
                    }

                    _logger.Fatal(ex, msg);
                }
                if (_isMessageProvider)
                {
                    _message.ErrorMessage(msg, ex);
                }
                else
                {
                    msg += $"\nСкопируйте и отправьте разработчику это сообщение";

                    msg = $"Exception: {msg}\n{ex.Message}\n{ex.StackTrace}";

#if !TEST
                    App.Document document = App.Application.DocumentManager.MdiActiveDocument;
                    if (document != null)
                    {
                        Editor editor = document.Editor;

                        editor.WriteMessage(msg);
                    }
                    else
                    {
                        App.Application.ShowAlertDialog(msg);
                    }
#else
                    Console.WriteLine(msg);
#endif
                }
            }
        }

        /// <summary>
        /// Запускает обновление и очистку служебных файлов в фоновом асинхронном процессе.
        /// </summary>
        /// <remarks>
        /// Задача намеренно не ожидается, чтобы не блокировать синхронную инициализацию CAD-хоста.
        /// Исключения, возникшие до передачи управления асинхронному методу, обрабатываются здесь;
        /// ошибки внутри задачи должны обрабатываться самим <see cref="UpdateAsync"/>.
        /// </remarks>
        private void StartUpdate()
        {
            try
            {
                // Создаём экземпляр класса UpdateAsync для выполнения обновления и очистки служебных файлов.
                UpdateAsync updateAsync = new UpdateAsync(_loggerFactory,
                                                            _message,
                                                            _messagePrompt);

                // Запускаем асинхронное обновление и очистку служебных файлов в фоновом процессе.
                _ = updateAsync.UpdateAsyncRun(UpdateLaunchMode.Automatic,
                                                _settings,
                                                _addOnInfo);
            }
            catch (Exception ex)
            {
                if (_isLoggerProvider)
                {
                    _logger.Error(ex);
                }
            }
        }

        /// <summary>
        /// Создаёт корень композиции при первом вызове и получает необходимые сервисы аддона.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Не удалось создать корень композиции или получить необходимые сервисы.
        /// </exception>
        private void InitializeServices()
        {
            try
            {
                // Проверяем, был ли уже создан корень композиции. Если нет, создаём его и регистрируем сервисы.
                if (!_isAddOnCompositionRoot)
                {
                    //***** РЕГИСТРИРУЕМ СЕРВИСЫ *************
                    // Регистрируем сервисы один раз при инициализации CAD-хоста.
                    AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(EntryPoint).Assembly);

                    // Передаём зарегистрированные сервисы в общий контекст аддона.
                    AddOnContext  /*AddOnCtx*/.Initialize(root.Get<IAddOnServices>());

                    _isAddOnCompositionRoot = true;// Корень композиции успешно инициализирован.
                }

                // Получаем фабрику логгеров из контейнера сервисов.
                _loggerFactory = AddOnContext.NLogFactory;
                _isLoggerFactoryProvider = true;

                // Получаем логгер для текущего класса из фабрики логгеров.
                _logger = _loggerFactory.GetLogger(typeof(EntryPoint));
                _isLoggerProvider = true;//логгер есть

                // Получаем сервис сообщений из контейнера сервисов.
                _message = AddOnContext.Msg;
                _isMessageProvider = true;  //ссобщения есть

                // Получаем сервис оконных сообщений из контейнера сервисов.
                _windowMessage = AddOnContext.MsgW;
                _isWindowMessageProvider = true;

                // Получаем сервис сообщений с запросом из контейнера сервисов.
                _messagePrompt = AddOnContext.MsgP;
                _isMessagePromptProvider = true;

                // Получаем настройки аддона из контейнера сервисов.
                _settings = AddOnContext.AddonSettings;
                _isSettingsProvider = true;

                // Получаем сведения о сборке аддона из контейнера сервисов.
                _addOnInfo = AddOnContext.AddOnInfo;
                _isAddOnInfoProvider = true;

                // Получаем сведения о CAD-хосте из контейнера сервисов.
                _cadInfo = AddOnContext.CadInfo;
                _isCadInfoProvider = true;

                // Получаем сведения о системе из контейнера сервисов.
                _sysInfo = AddOnContext.SysInfo;
                _isSysInfoProvider = true;
            }
            catch (Exception ex)
            {
                // Если возникла ошибка при инициализации сервисов, логируем её и выбрасываем исключение.
                if (_isLoggerProvider)
                {
                    _logger.Fatal(ex);
                }

                // Выбрасываем исключение, чтобы сигнализировать о неудачной инициализации корня композиции.
                throw new InvalidOperationException("AddOnCompositionRoot initialization failed", ex);
            }
        }

        /// <summary>
        /// Записывает сведения об аддоне и CAD-хосте в журнал и выводит приветственное сообщение.
        /// </summary>
        private void ReportInitialization()
        {
            try
            {
                _logger.Trace(_addOnInfo.ToLongString());
                _logger.Trace(_cadInfo.ToLongString());
                _logger.Trace(_sysInfo.ToLongString());

                _message.InfoMessage($"Hello {_addOnInfo.ToShortString()} for {_cadInfo.ToString()}");

#if !TEST
                // Показываем сведения о зарегистрированных командах только при включённой настройке.
                if (_isSettingsProvider && _settings.General.ShowCommandInfoOnStartup)
                {
                    CommandInfo.CmdInfo();
                }
#endif
            }
            catch (Exception ex)
            {
                if (_isLoggerProvider)
                {
                    _logger.Error(ex, ex.Message);
                }
            }
        }

        /// <summary>
        ///  Выполняет завершающие действия при выгрузке аддона из CAD-хоста.
        /// </summary>
        public void Terminate()
        {
            if (_isLoggerProvider)
            {
                _logger.Trace("Terminate");
            }

            // Сохранение настроек здесь не выполняется: состояние настроек управляется отдельным сервисом.
            //dsf.SaveDataSet();
            //dsf.SetGeneralSetting.WriteValue("TEST", "terminate", "true");
        }
    }
}