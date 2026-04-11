using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using drz.DiContainer;
using drz.Loader.Infrastructure;
using NLog;
using System.Reflection;

namespace drz.SpecSPDS
{
    public class CommandA
    {
        private ILogger log = LoggerProvider.For<CommandA>();
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