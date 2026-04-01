using dRz.SpecSpds.Test.Loader;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dRz.SpecSpds.Test.Tests
{
    internal class LogTests
    {
          ILogger  log = LoggerProvider.For<LogTests>();

        internal void LogTest(string msg)
        {
            log.Info(msg);

        }

    }
}
