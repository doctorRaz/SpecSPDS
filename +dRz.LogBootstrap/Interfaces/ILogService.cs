using System;

namespace dRz.LogServices.Interfaces
{
    public interface ILogService
    {
        NLog.Logger GetLogger<T>();
        NLog.Logger GetLogger(Type type);
    }
}