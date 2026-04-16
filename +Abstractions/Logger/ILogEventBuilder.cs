using System;
using System.Collections.Generic;

namespace drz.Abstractions.Logger
{
    public interface ILogEventBuilder
    {
        #region Public Methods

        void Log();

        ILogEventBuilder Message(string message);

        ILogEventBuilder Property(string name, object value);

        ILogEventBuilder Exception(Exception exception);

        ILogEventBuilder Properties(IEnumerable<KeyValuePair<string, object>> properties);


        #endregion Public Methods
    }
}