using drz.Abstractions.Logger;
using System;
using System.Collections.Generic;

namespace drz.LogBootstrap.drzNlog
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
        public ILogEventBuilder Properties(IEnumerable<KeyValuePair<string, object>> properties)
        {
            if (properties == null)
            {
                return this;
            }

            foreach (KeyValuePair<string, object> kvp in properties)
            {
                _inner.Property(kvp.Key, kvp.Value);
            }
            return this;
        }



        #endregion Public Methods
    }
}