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
        private ILogger log = LoggerProvider.For<CommandB>();

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