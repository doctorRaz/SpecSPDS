using NLog;
using dRz.SpecSPDS.Core.InternalDiagnostic;
using System;
using dRz.SpecSPDS.Core.Bootstrap;
using System.ComponentModel;
using dRz.SpecSPDS.Services;
using dRz.SpecSPDS.Interfaces;
using dRz.SpecSPDS.AssemblyResolve;
using dRz.SpecSPDS;









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

                AssemblyResolver resolver = new AssemblyResolver();

                //add  event Assembly resolve  
                resolver.Register();

                InitLoger();

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
            log.Info("LogManager.Shutdown");

            LogManager.Shutdown();
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