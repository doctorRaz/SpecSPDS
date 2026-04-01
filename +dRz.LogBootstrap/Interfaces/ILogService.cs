using System;

namespace dRz.Log.Interfaces
{
    public interface ILogService
    {
        NLog.Logger GetLogger<T>();
        NLog.Logger GetLogger(Type type);
    }
}