//global using AddOnCtx = dRz.Src.Infrastructure.AddOnContext;//глобально на сборку, чтобы не писать каждый раз длинный путь

// -----------------
using dRz.Abstractions.Infrastructure;
using dRz.Abstractions.Logger;
using dRz.Abstractions.Services;
using dRz.Abstractions.Services.Message;
using dRz.Abstractions.Settings;
using dRz.AddOnRuntime;
using dRz.Src.Infrastructure;
using dRz.Updater.Services;
using System;

#if NC

using HostMgd.ApplicationServices;
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

        private IAddonSettings _settings;

        private bool _isRegisterAssemblyResolver;// Флаг регистрации обработчика разрешения сборок.

        private IDrzLogger _logger;
        private static bool _isLoggerProvider;// Логгер успешно получен.

        private IMessageService _message;
        private bool _isMessageProvider;// Сервис пользовательских сообщений получен.

        private IAddOnInfo _addOnInfo;// Сведения о сборке аддона.
        private bool _isAddOnInfoProvider;

        private ICadInfo _cadInfo;// Сведения о CAD-хосте.
        private bool _isCadInfoProvider;

        private ISysInfo _sysInfo;//о cad
        private bool _isSysInfoProvider;

#if DEBUG&&!TEST

        [Rtm.CommandMethod(CommandPrefix.Value + "INIT", Rtm.CommandFlags.UsePickSet)]
        public static void test()
        {
            EntryPoint entryPoint = new EntryPoint();
            entryPoint.Initialize();
        }

#endif

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
                // регистрируемся
                TryAddOnCompositionRoot();// Получаем сервисы и контекст аддона.

                // Загружаем настройки сразу при инициализации и фиксируем singleton в контейнере.
                _settings = AddOnContext.Services.Get<IAddonSettings>();

                _addOnInfo = AddOnContext.Services.Get<IAddOnInfo>();
                _isAddOnInfoProvider = true;

                _cadInfo = AddOnContext.Services.Get<ICadInfo>();
                _isCadInfoProvider = true;

                _sysInfo = AddOnContext.Services.Get<ISysInfo>();
                _isSysInfoProvider = true;

                if (!_addOnInfo.IsProductDirectory)
                {
                    AddOnContext.Services.Get<IWindowMessageService>().WarningMessage(
                        $"Приложение не в своем родном каталоге" +
                        $"\n{_addOnInfo.PackageDirectory}" +
                        $"\nВозможны ошибки в работе!");
                }

                //стартуем обновление, очистку копий и bak
                TryUpdate();

                // служебные записи и сообщение пользователю, что аддон поднялся
                TryInit(_addOnInfo.ToLongString(),
                        _cadInfo.ToLongString(),
                        _sysInfo.ToLongString());
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
                    Document document = Application.DocumentManager.MdiActiveDocument;
                    if (document != null)
                    {
                        Editor editor = document.Editor;

                        editor.WriteMessage(msg);
                    }
                    else
                    {
                        Application.ShowAlertDialog(msg);
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
        private void TryUpdate()
        {
            try
            {
                UpdateAsync updateAsync = new UpdateAsync();
                _ = updateAsync.UpdateAsyncRun(UpdateLaunchMode.Automatic);
            }
            catch (Exception ex)
            {
                if (_isLoggerProvider)
                    _logger.Error(ex);
            }
        }

        /// <summary>
        /// Создаёт корень композиции при первом вызове и получает необходимые сервисы аддона.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Не удалось создать корень композиции или получить необходимые сервисы.
        /// </exception>
        private void TryAddOnCompositionRoot()
        {
            try
            {
                if (!_isAddOnCompositionRoot)
                {
                    //***** РЕГИСТРИРУЕМ СЕРВИСЫ *************
                    // Регистрируем сервисы один раз при инициализации CAD-хоста.
                    AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(EntryPoint).Assembly);

                    // Передаём зарегистрированные сервисы в общий контекст аддона.
                    AddOnContext  /*AddOnCtx*/.Initialize(root.Get<IAddOnServices>());

                    _isAddOnCompositionRoot = true;// Корень композиции успешно инициализирован.

                }


                //логер для класса
                _logger = AddOnContext.Services.Get<IDrzLoggerFactory>().GetLogger(typeof(EntryPoint));
                _isLoggerProvider = true;//логгер есть

                _message = AddOnContext.Services.Get<IMessageService>()/* AddOnCtx.Msg*/;
                _isMessageProvider = true;  //ссобщения есть


            }
            catch (Exception ex)
            {
                //если логер получен пытаемся сохранить ошбку в лог
                if (_isLoggerProvider)
                {
                    _logger.Fatal(ex);
                }

                // Преобразуем ошибку в исключение инициализации с исходной причиной.
                throw new InvalidOperationException("AddOnCompositionRoot initialization failed", ex);
            }
        }

        /// <summary>
        /// Записывает сведения об аддоне и CAD-хосте в журнал и выводит приветственное сообщение.
        /// </summary>
        private void TryInit(string addOnInfo,
                            string cadInfo,
                            string sysInfo)
        {
            try
            {
                _logger.Trace(addOnInfo);
                _logger.Trace(cadInfo);
                _logger.Trace(sysInfo);

                _message.InfoMessage($"Hello {_addOnInfo.ToShortString()} for {_cadInfo.ToString()}");
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
        /// Выполняет завершающие действия при выгрузке аддона из CAD-хоста.
        /// </summary>
        public void Terminate()
        {
            if (_isLoggerProvider)
                _logger.Trace("Terminate");

            // Сохранение настроек здесь не выполняется: состояние настроек управляется отдельным сервисом.
            //dsf.SaveDataSet();
            //dsf.SetGeneralSetting.WriteValue("TEST", "terminate", "true");
        }
    }
}