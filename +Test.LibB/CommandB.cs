using drz.Abstractions.Logger;
using drz.AddOn.Composition;
using drz.Src.Infrastructure;
using drz.Src.Services;
using System;

using /*static*/ AC = drz.Src.Infrastructure.AddOnContext;

namespace drz.Lib_B
{
    public class CommandB : IDisposable
    {
        private static bool _initialized;

        private IDrzLogger log = LoggerProvider.For<CommandB>();

        public CommandB()
        {
            if (_initialized)
            {
                return;
            }

            AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(CommandB).Assembly);

            AC.Initialize(root);

            _initialized = true;
        }

        public void Dispose()
        {
            AC.Dispose();
        }

        public string ExecuteEnvironmentInfoProvider()
        {
            CadEnvironmentInfoProvider cadEnvironmentInfoProvider = new CadEnvironmentInfoProvider();

            return cadEnvironmentInfoProvider.GetSummary();
        }

        public void LogTest(string msg)
        {
            log.Trace($"Сообщение Trace");
            log.Debug($"Сообщение Debug");
            log.Info($"Сообщение Info");
            log.Warn($"Сообщение Warn");
            log.Error($"Сообщение Error");
            log.Fatal($"Сообщение Fatal");
            log.Fatal($"{msg}");
        }

        public void msgCommandB()
        {
            AC.MsgCmd.ConsoleMessage($"{AC.InfoDll.TitlePrefix} Console message");

            AC.MsgGUI.InfoMessage($"{AC.InfoDll.TitlePrefix} Console message");

            AC.Msg.InfoMessage($"{AC.InfoDll.TitlePrefix} Console message");
        }
    }
}