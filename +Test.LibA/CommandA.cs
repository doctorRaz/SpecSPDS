using drz.AddOn.Composition;
using drz.Src.Infrastructure;

using NLog;
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
        //private readonly AddOnCompositionRoot _di;

        private ILogger log = LoggerProvider.For<CommandA>();

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
            CadEnvironmentInfoProvider ff = new CadEnvironmentInfoProvider();

            return ff.GetSummary();
        }

        public void LogTest(string msg)
        {
            log.Info(msg);
        }

        public void msgCommandA()
        {
            AC.MsgCmd.ConsoleMessage($"{AC.AddOn.TitlePrefix} Console message");

            AC.MsgGUI.InfoMessage($"{AC.AddOn.TitlePrefix} Console message");

            AC.Msg.InfoMessage($"{AC.AddOn.TitlePrefix} Console message");
        }
    }
}