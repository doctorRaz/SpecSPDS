using drz.Abstractions.Logger;
using drz.AddOn.Composition;
using drz.Src.Infrastructure;
using drz.Src.Services;
using System;

using /*static*/ AC = drz.Src.Infrastructure.AddOnContext;

/// <summary>
///
/// </summary>
namespace drz.Lib_A
{
    public class CommandA : IDisposable
    {
        private static bool _initialized;

        private IDrzLogger log = LoggerProvider.For<CommandA>();

        public CommandA()
        {
            if (_initialized)
            {
                return;
            }

            AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(CommandA).Assembly);

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

        public void msgCommandA()
        {
            AC.MsgCmd.ConsoleMessage($"{AC.AddonInfo.TitlePrefix} Console message");

            AC.MsgGUI.InfoMessage($"{AC.AddonInfo.TitlePrefix} Console message");

            AC.Msg.InfoMessage($"{AC.AddonInfo.TitlePrefix} Console message");
        }
    }
}