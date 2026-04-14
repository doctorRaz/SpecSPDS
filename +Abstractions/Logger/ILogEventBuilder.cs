using System;

namespace drz.Abstractions.Logger
{
    public interface ILogEventBuilder
    {
        #region Public Methods

        void Log();

        ILogEventBuilder Message(string message);

        ILogEventBuilder Property(string name, object value);

        #endregion Public Methods
    }
}