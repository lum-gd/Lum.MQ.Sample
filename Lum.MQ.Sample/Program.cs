using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;

namespace Lum.MQ.Sample
{
    public class Program
    {
        public static int Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.ison")
                .AddCommandLine(args)
                .Build();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Async(a => a.File(config["LogFile"]))
                .WriteTo.Debug()
                //.WriteTo.SQLite("Logs/log.db".CreateLogger();
                .CreateLogger();
            try
            {
                Log.Information("App starting. ----------------------------");
                CreateHostBuilder(args).Build().Run();
                Log.Information("App end. ==============================");
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "App Crashed. =======================");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
    }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
