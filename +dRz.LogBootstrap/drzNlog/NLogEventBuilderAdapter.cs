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
        #region Private Fields

        private readonly NLog.LogEventBuilder _inner;

        #endregion Private Fields

        #region Internal Constructors

        internal NLogEventBuilderAdapter(NLog.LogEventBuilder inner)
        {
            _inner = inner;
        }

        #endregion Internal Constructors

        #region Public Methods

        public ILogEventBuilder Exception(Exception exception)
        {
            _inner.Exception(exception);
            return this;
        }

        public void Log() => _inner.Log();

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

        #endregion Public Methods
    }
}