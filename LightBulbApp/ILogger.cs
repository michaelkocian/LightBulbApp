using System;

namespace LightBulbApp
{
    public interface ILogger
    {
        void LogWarning(string v);
        void LogInformation(string v);
        void LogError(Exception e, string v, string cmd);
        void LogDebug(string v);
    }
}