using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using drz.DiContainer;
using drz.Loader.Infrastructure;
using NLog;
using System.Reflection;
using static drz.DiContainer.DiRegister;

namespace drz.SpecSPDS
{
    public class CommandB
    {
        public static   SimpleInjector.Container ContainerIn;
        public CommandB()
        {
            var dr = new DiRegister(Assembly.GetExecutingAssembly());

             ContainerIn = dr.ContainerIn;
        }


        public void msgCommandB()
        {

            IApplicationInfo app = ContainerIn.GetInstance<IApplicationInfo>();
            IMessageService messageService = ContainerIn.GetInstance<ICommandLineMessageService>();
            messageService.ConsoleMessage($"{app.TitlePrefix} Console message");

            messageService = ContainerIn.GetInstance<IWindowMessageService>();
            messageService.InfoMessage("Info message");

        }

        private ILogger log;//= LoggerProvider.For<CommandB>();

        public string Execute()
        {
            CadEnvironmentInfoProvider ff = new CadEnvironmentInfoProvider();

            return ff.GetSummary();
        }

        public void LogTest(string msg)
        {

            log.Info(msg);

        }
    }
}