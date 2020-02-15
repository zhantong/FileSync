using Microsoft.Extensions.Logging;

namespace FileSync.Core
{
    static class CoreLogger
    {
        public static ILoggerFactory CoreLoggerFactory;

        static CoreLogger()
        {
            CoreLoggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddConsole();
            });
        }
    }
}