using drz.AddOn.Composition;
using drz.Src.Infrastructure;
using NLog;
using System;

using /*static*/ AC = drz.Src.Infrastructure.AddOnContext;

namespace drz.Lib_B
{
    public class CommandB : IDisposable
    {
        private static bool _initialized;

        private ILogger log = LoggerProvider.For<CommandB>();

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

        public string Execute()
        {
            CadEnvironmentInfoProvider ff = new CadEnvironmentInfoProvider();

            return ff.GetSummary();
        }

        public void LogTest(string msg)
        {
            log.Info(msg);
        }

        public void msgCommandB()
        {
            AC.MsgCmd.ConsoleMessage($"{AC.AddOn.TitlePrefix} Console message");

            AC.MsgGUI.InfoMessage("Info message");

            //IApplicationInfo app = AC.Get<IApplicationInfo>();
            //IMessageService messageService = AC.Get<ICommandLineMessageService>();
            //messageService.ConsoleMessage($"{app.TitlePrefix} Console message");

            //messageService = AC.Get<IWindowMessageService>();
            //messageService.InfoMessage("Info message");
        }
    }
}