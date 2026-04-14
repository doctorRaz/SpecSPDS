using drz.Abstractions.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drz.LogServices.drzNlog
{
    internal sealed class NLogEventBuilderAdapter : ILogEventBuilder
    {
        private readonly NLog.LogEventBuilder _inner;

        internal NLogEventBuilderAdapter(NLog.LogEventBuilder inner)
        {
            _inner = inner;
        }

        public ILogEventBuilder Message(string message)
        {
            _inner.Message(message);
            return this;
        }

        public ILogEventBuilder Property(string name, object value)
        {
            _inner.Property(name, value);
            return this;
        }

        public void Log() => _inner.Log();
    }
}