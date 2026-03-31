
using NLog;
using System;
using System.ComponentModel;
using dRz.SpecSPDS.Services;
using dRz.SpecSPDS.Interfaces;
using dRz.SpecSPDS;
using dRz.Cad.Diagnostics.Os;
using static dRz.SpecSPDS.Infrastructure.AddonContext;
using dRz.Cleaner.Infrastructure;
using dRz.Cad.Diagnostics;





#if AC

using Rtm = Autodesk.AutoCAD.Runtime;

#elif NC

using Rtm = Teigha.Runtime;
#endif

[assembly: Rtm.ExtensionApplication(typeof(EntryPoint))]

namespace dRz.SpecSPDS
{
    public /*partial*/ class EntryPoint : Rtm.IExtensionApplication
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private IMessageService msg = new MessageService();

#if DEBUG
        [Rtm.CommandMethod("инитАд")]
        [Description("ручной инит адаптера")]
#endif
        public void Initialize()
        {
            //если нет библиотек или еще какой косяк
            try
            {
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
                log.Fatal(ex, ex.Message);
            }
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
                CleaningBackups.Cleaning(InfoDll.AssemblyDirectory);
            }
            catch { }
        }

        #endregion


        private void TryInit()
        {
            try
            {

                log.Debug("AdOn: {0}", InfoDll);

                log.Debug("CAD: {0}", RT.Cad);

                log.Debug("OS: {0}", InfoOs.Current);

                msg.ConsoleMessage($"Hello SpecSPDS for {RT.Cad}");


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