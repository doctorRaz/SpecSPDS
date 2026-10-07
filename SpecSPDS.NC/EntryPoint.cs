global using AddOnCtx = dRz.Src.Infrastructure.AddOnContext;//глобально на сборку, чтобы не писать каждый раз длинный путь


// -----------------
using dRz.Abstractions.Infrastructure;
using dRz.Abstractions.Logger;
using dRz.Abstractions.Services;
using dRz.Abstractions.Services.Message;
using dRz.AddOnRuntime;
using dRz.Updater.Services;
using drz.MulticadInterop;


#if NC

using HostMgd.ApplicationServices;
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
    /// <summary> инициализация модуля </summary>
    public class EntryPoint : Rtm.IExtensionApplication
    {
        private static bool _isAddOnCompositionRoot;//контейнер наполнен

        private bool _isRegisterAssemblyResolver;//register assembly resolver

        private IDrzLogger _logger;
        private static bool _isLoggerProvider;//логер есть

        private IMessageService _message;
        private bool _isMessageProvider;//сообщения

        private IAddOnInfo _addOnInfo;//о сборке
        private bool _isAddOnInfoProvider;

        private ICadInfo _cadInfo;//о cad
        private bool _isCadInfoProvider;

        private ISysInfo _sysInfo;//о cad
        private bool _isSysInfoProvider;

#if DEBUG

        [Rtm.CommandMethod("инитПЧ", Rtm.CommandFlags.UsePickSet)]
        public static void test()
        {
            EntryPoint entryPoint = new EntryPoint();
            entryPoint.Initialize();
        }

#endif

        /// <summary>Initializes this instance.</summary>
        public void Initialize()
        {
            try
            {
                // регистрируемся
                MulticadNotificatorDiagnostic.DumpCreateMessage();
                TryAddOnCompositionRoot();//получаем окружение

                //стартуем очистку копий и bak
                TryCleanBackups();

                //
                TryInit();
            }
            catch (Exception ex) // ошибка инициализации, все развалилось, лог смысла не имеет
            {
                string message = $"Приложение не загружено!!!";

                if (_isLoggerProvider)//todo если лог инит ПРОВЕРИТЬ не вызовет ли еще один ЕХ если false??!!
                {
                    message += $"\nОтправьте разработчику лог файлы из каталога [APPDATA/ЭТО_ПРИЛОЖЕНИЕ/Logs]";

                    _logger.Error(ex, message);
                }
                if (_isAddOnCompositionRoot)
                {
                    _message.ErrorMessage(message, ex);
                }
                else
                {
#if NC
                    message += $"\nСкопируйте и отправьте разработчику это сообщение";

                    message = $"Exception: {message}\n{ex.Message}\n{ex.StackTrace}";

                    Document document = App.Application.DocumentManager.MdiActiveDocument;
                    if (document != null)
                    {
                        Editor editor = document.Editor;

                        editor.WriteMessage(message);
                    }
                    else
                    {
                        App.Application.ShowAlertDialog(message);
                    }
#endif
                }
            }
        }

        /// <summary>Tries the clean backups.</summary>
        private void TryCleanBackups()
        {
            try
            {
                CleanBackups();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"CleanBackups: {ex.Message}");
            }
        }

        /// <summary>Cleans the backups.</summary>
        private void CleanBackups()
        {
            try
            {
                BackupCleaner.DeleteBackupFiles(AddOnCtx.AddOnInfo.PackageDirectory);
            }
            catch { }
        }

        /// <summary>Tries the add on composition root.</summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">AddOnCompositionRoot initialization failed</exception>
        private void TryAddOnCompositionRoot()
        {
            try
            {
                if (!_isAddOnCompositionRoot)
                {
                    //***** РЕГИСТРИРУЕМ СЕРВИСЫ *************
                    // один раз в точке входа /Rtm.IExtensionApplication/
                    AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(EntryPoint).Assembly);

                    // экземпляр копии контейнера by ref
                    AddOnCtx.Initialize(root.Get<IAddOnServices>());

                    _isAddOnCompositionRoot = true;//сервис поднялся
                }

                //логер для класса
                _logger = AddOnCtx.NLogFactory.GetLogger(typeof(EntryPoint));
                _isLoggerProvider = true;//логгер есть

                _message = AddOnCtx.Msg;
                _isMessageProvider = true;

                _addOnInfo = AddOnCtx.AddOnInfo;
                _isAddOnInfoProvider = true;

                _cadInfo = AddOnCtx.CadInfo;
                _isCadInfoProvider = true;

                _sysInfo = AddOnCtx.SysInfo;
                _isSysInfoProvider = true;
            }
            catch (Exception ex)
            {
                //роняем загрузчик
                throw new InvalidOperationException("AddOnCompositionRoot initialization failed", ex);
            }
        }

        /// <summary>Tries the initialize.</summary>
        private void TryInit()
        {
            try
            {
                _logger.Info(AddOnCtx.AddOnInfo.ToLongString());

                _message.InfoMessage($"Hello {AddOnCtx.AddOnInfo.ToShortString()} for {AddOnCtx.CadInfo.ToString()}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
            }
        }

        /// <summary>Terminates this instance.</summary>
        public void Terminate()
        {
            _logger.Trace("Terminate");

            //think грузить в инит, теряет свойства радиобатонов
            //dsf.SaveDataSet();
            //dsf.SetGeneralSetting.WriteValue("TEST", "terminate", "true");
        }
    }
}