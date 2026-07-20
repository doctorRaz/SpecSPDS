using System;
using System.Collections.Generic;

namespace drz.Abstractions.Logger
{
    public interface ILogEventBuilder
    {
        #region Public Methods
        void Log();

        ILogEventBuilder Exception(Exception exception);

        ILogEventBuilder Message(string message);

        ILogEventBuilder Properties(IEnumerable<KeyValuePair<string, object>> properties);

        ILogEventBuilder Property(string name, object value);
        
        #endregion Public Methods
    }
}