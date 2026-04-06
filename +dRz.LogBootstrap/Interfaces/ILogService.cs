using NLog;
using System;

namespace dRz.LogServices.Interfaces
{
    public interface ILogService
    {
        Logger GetLogger<T>();
        Logger GetLogger(Type type);
    }
}