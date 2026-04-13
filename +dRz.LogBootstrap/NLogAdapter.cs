//https://replit.com/@razygraev/Log-Service-interface

using NLog;
using System;
using drz.LogServices.Interfaces;
using drz.Abstractions.Logger;

namespace drz.LogServices
{
    public class NLogAdapter : IDrzLogger
    {
        private readonly NLog.Logger _logger;

        public NLogAdapter(NLog.Logger logger)
        {
            _logger = logger;
        }

        public void Debug(string message) => _logger.Debug(message);
        public void Info(string message) => _logger.Info(message);
        public void Warn(string message) => _logger.Warn(message);
        public void Error(string message, Exception exception = null) =>
            _logger.Error(exception, message);
        public void Fatal(string message, Exception exception = null) =>
            _logger.Fatal(exception, message);

        public IDrzLogger GetLogger<T>()
        {
            throw new NotImplementedException();
        }

        public IDrzLogger GetLogger(Type type)
        {
            throw new NotImplementedException();
        }
    }

    public class NLogService : IDrzLogService
    {
        public void Debug(string message)
        {
            throw new NotImplementedException();
        }

        public void Error(string message, Exception exception = null)
        {
            throw new NotImplementedException();
        }

        public void Fatal(string message, Exception exception = null)
        {
            throw new NotImplementedException();
        }

        public IDrzLogger GetLogger<T>() =>
            new NLogAdapter(LogManager.GetLogger(typeof(T).FullName));

        public IDrzLogger GetLogger(Type type) =>
            new NLogAdapter(LogManager.GetLogger(type.FullName));

        public void Info(string message)
        {
            throw new NotImplementedException();
        }

        public void Warn(string message)
        {
            throw new NotImplementedException();
        }
    }
}