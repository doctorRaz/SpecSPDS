global using AddOnCtx = dRz.Src.Infrastructure.AddOnContext;//глобально на сборку, чтобы не писать каждый раз длинный путь

// -----------------
using dRz.Abstractions.Infrastructure;
using dRz.Abstractions.Logger;
using dRz.Abstractions.Services;
using dRz.Abstractions.Services.Message;
using dRz.Abstractions.Settings;
using dRz.AddOnRuntime;
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
                TryAddOnCompositionRoot();//получаем окружение

                //стартуем обновление, очистку копий и bak
                TryUpdate();

                //
                TryInit();
            }
            catch (Exception ex) // ошибка инициализации, все развалилось, лог смысла не имеет
            {
                string msg = $"Приложение не загружено!!!";

                if (_isLoggerProvider)//todo если логер есть ПРОВЕРИТЬ не вызовет ли еще один ЕХ если false??!!
                {
                    msg += $"\nОтправьте разработчику логфайлы";

                    if (_isAddOnInfoProvider)
                    {
                        msg += $": {_addOnInfo.ProductDataDirectory}\\logs";
                    }

                    _logger.Error(ex, msg);
                }
                if (_isMessageProvider)
                {
                    _message.ErrorMessage(msg, ex);
                }
                else
                {
#if NC
                    msg += $"\nСкопируйте и отправьте разработчику это сообщение";

                    msg = $"Exception: {msg}\n{ex.Message}\n{ex.StackTrace}";

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
#endif
                }
            }
        }

        /// <summary>Tries the clean backups.</summary>
        private void TryUpdate()
        {
            try
            {
                UpdateAsync updateAsync = new UpdateAsync();
                _ = updateAsync.UpdateAsyncRun(UpdateLaunchMode.Automatic);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"CleanBackups: {ex.Message}");
            }
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

                    // Загружаем настройки сразу при инициализации и фиксируем singleton в контейнере.
                    _ = AddOnCtx.Services.Get<IAddonSettings>();

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
                //если логер получен пытаемся сохранить ошбку в лог
                if (_isLoggerProvider)
                {
                    _logger.Fatal(ex);
                }

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
                _logger.Info(AddOnCtx.CadInfo.ToLongString());
                _logger.Info(AddOnCtx.SysInfo.ToLongString());

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