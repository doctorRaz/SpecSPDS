using drz.AddOn.Composition;
using drz.Loader.Infrastructure;

using NLog;
using System;

using /*static*/ AC = drz.Loader.Infrastructure.AddOnContext;

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

        public string Execute()
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

            AC.MsgGUI.InfoMessage("Info message");

            // Если нужно Scoped-сервисы, можно обернуть в using
            //using (AC.BeginScope())
            //{
            //    AC.MsgCmd.ConsoleMessage($"{AC.AddOn.TitlePrefix} Console message");

            //    AC.MsgGUI.InfoMessage("Info message");

            //    var app1 = AC.Get<IApplicationInfo>();

            //    IMessageService messageService1 = AC.Get<ICommandLineMessageService>();

            //    messageService1.ConsoleMessage($"{app1.TitlePrefix} Console message");

            //    messageService1 = AC.Get<IWindowMessageService>();
            //    messageService1.InfoMessage("Info message");
            //}

            //IApplicationInfo app =  AC.Get<IApplicationInfo>();

            //IMessageService messageService = AC.Get<ICommandLineMessageService>();

            //messageService.ConsoleMessage($"{app.TitlePrefix} Console message");

            //messageService = AC.Get<IWindowMessageService>();
            //messageService.InfoMessage("Info message");
        }
    }
}