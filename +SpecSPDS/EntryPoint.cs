using NLog;
using dRz.SpecSPDS.Core.InternalDiagnostic;
using System;
using dRz.SpecSPDS.Core.Bootstrap;
using System.ComponentModel;
using dRz.SpecSPDS.Services;
using dRz.SpecSPDS.Interfaces;
using dRz.SpecSPDS;
using dRz.CAD.Runtime.Info;
using static dRz.SpecSPDS.Infrastructure.AddonContext;










#if AC

using Rtm = Autodesk.AutoCAD.Runtime;

#elif NC

using App = HostMgd.ApplicationServices.Application;
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

                //todo прибить класс               AssemblyResolver resolver = new AssemblyResolver();

                //add  event Assembly resolve  
                //todo прибить класс      //resolver.Register();

                //todo прибить класс     InitLoger();

                log.Debug("AdOn: {0}", InfoDll);

                log.Debug("CAD: {0}", new InfoCad());

                log.Debug("OS: {0}", InfoOs.Current);

                InitAdapter();
            }
            catch (Exception ex)
            {

                msg.ExceptionMessage(ex);
            }
        }

        private void InitLoger()
        {
            try
            {

                //если лог конфиг не загрузился сам грузим руками
                if (LogManager.Configuration is null)
                {
                    //пытаемся грузить принудительно
                    new LogBootstrap();

                    //если конфиг не нашелся и не загрузился
                    if (LogManager.Configuration is null)
                    {
                        //включим диагностику eсли выключена
                        new InternalLoggerDiagnostic("LogManager empty Configuration");

                        //дальше пишем внутренний лог
                    }
                }

                log.Info("Logger Started");

            }
            catch (Exception ex)
            {
                msg.ExceptionMessage(ex);
            }
        }

        private void InitAdapter()
        {
            Loader loader = new Loader();
            loader.HelloSpec();
        }

        public void Terminate()
        {
            log.Debug("LogManager.Shutdown");

            //LogManager.Shutdown();
        }
    }

    class Loader
    {
        private IMessageService msg = new MessageService();
        internal void HelloSpec()
        {
            msg.ConsoleMessage($"Hello SpecSPDS for nanoCAD v{App.Version.Major}.{App.Version.Minor}");
        }
    }



}