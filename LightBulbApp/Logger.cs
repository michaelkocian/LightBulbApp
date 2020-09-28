using System;

namespace LightBulbApp
{
    class Logger : ILogger
    {
        public void LogDebug(string v)
        {
            //  Console.WriteLine(v);
        }

        public void LogError(Exception e, string v, string cmd)
        {
            Console.WriteLine(v);
        }

        public void LogInformation(string v)
        {
            Console.WriteLine(v);
        }

        public void LogWarning(string v)
        {
            Console.WriteLine(v);
        }
    }
}
