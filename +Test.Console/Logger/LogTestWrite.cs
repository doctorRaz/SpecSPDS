using drz.Abstractions.Logger;
using drz.Src.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace drz.SpecSpds.Test.Logger
{
    internal class LogTestWrite
    {
        private IDrzLogger log = LoggerProvider.For<LogTestWrite>();

        internal void Test()
        {
            log.Trace($"Сообщение Trace");
            log.Debug($"Сообщение Debug");
            log.Info($"Сообщение Info");
            log.Warn($"Сообщение Warn");
            log.Error($"Сообщение Error");
            log.Fatal($"Сообщение Fatal");
        }
    }
}