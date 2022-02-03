using Serilog;

namespace Lumin.MQ.Rabbit.WorkerSample
{
    public static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder ConfigureSerilog(this ILoggingBuilder loggingBuilder, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Async(a => a.File("Logs/log.txt"))
                .WriteTo.Debug()
                .CreateLogger();

            return loggingBuilder;
        }
    }
}