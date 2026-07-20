using System;
using System.ComponentModel;
using drz.SpecSPDS.Services;
using drz.SpecSPDS.Interfaces;
using drz.SpecSPDS;
using static drz.Src.Infrastructure.AddOnContext;
using drz.Abstractions.Logger;
using drz.Src.Services;
using drz.Updater.Services;






#if AC

using Rtm = Autodesk.AutoCAD.Runtime;

#elif NC

using Rtm = Teigha.Runtime;

#endif

[assembly: Rtm.ExtensionApplication(typeof(EntryPoint))]

namespace drz.SpecSPDS
{
    public class EntryPoint : Rtm.IExtensionApplication
    {
        private IDrzLogger? log;

        private IMessageService? msg;

#if DEBUG

        [Rtm.CommandMethod("инитСП")]
        [Description("ручной инит адаптера")]
        public static void test()
        {
            IMessageService msg = new MessageService();
            msg.ConsoleMessage($"инит адаптера");
            EntryPoint entryPoint = new EntryPoint();
            entryPoint.Initialize();
        }

#endif

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            //если нет библиотек или еще какой косяк
            try
            {
            //todo нет симплеинжектора, все валится

                //если ех тут то все пропало
                TryMessageService();

                //обертка инит логера, если ех на старте, то отловим и напишем в месадж
                TryLoggerProvider();

                //стартуем очистку копий и bak
                TryCleanBackups(); //независимо от результата чистки, работа аддона будет продолжена
                                   //ошибку сюда не поднимаем

                //
                TryInit();
            }
            catch (Exception ex)
            {
                string message = $"Приложение не загружено!!!" +
                                $"\nСкопируйте это сообщение и отправьте разработчику";
                msg.ExceptionMessage(message, ex);
                //log.Fatal(ex, ex.Message); здесь нельзя писать в лог
            }
        }

        /// <summary>
        /// Tries the message service.
        /// </summary>
        private void TryMessageService()
        {
            try
            {
                msg = new MessageService();
            }
            catch { throw; }//роняем аддон
        }

        /// <summary>
        /// Tries the logger.
        /// </summary>
        private void TryLoggerProvider()
        {
            try
            {
                log = LoggerProvider.For<EntryPoint>();
            }
            catch { throw; }//роняем аддон
        }

        #region CleaningBackups

        /// <summary>
        /// Cleans the backups.
        /// </summary>
        private void TryCleanBackups()
        {
            try //если ех на входе в библиотеку, то мы поймаем это исключение
                //запишем и работаем дальше
            {
                CleanBackups();
            }
            catch (Exception ex)
            {
                log.Error(ex, $"CleanBackups: {ex.Message}");
            }
        }

        private void CleanBackups()
        {
          

            try//игнорим ошибки
            {
                BackupCleaner.DeleteBackupFiles(AddonInfo.AssemblyDirectory);
            }
            catch { }
        }

        #endregion CleaningBackups

        private void TryInit()
        {
            try
            {
                log.Debug(SysInfo.ToString());

                log.Debug(CadInfo.ToString());

                log.Debug(AddonInfo.ToString());

                msg.ConsoleMessage($"Hello {AddonInfo.ToShortString()} for {CadInfo}");

                TryListCMD();

                TryUpdater();
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Tries the list command.<br/>
        /// Вывод списка зарегистрированных команд и инфы о аддоне
        /// </summary>
        private void TryListCMD()
        {
            try
            {
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Tries the updater.<br/>
        /// Задел для обновлятора
        /// </summary>
        private void TryUpdater()
        {
            try
            {
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
            }
        }

        public void Terminate()
        {
            try
            {
                log.Debug("Terminate");
            }
            catch { }// смысла нет что то показывать при закрытии наны
        }
    }
}