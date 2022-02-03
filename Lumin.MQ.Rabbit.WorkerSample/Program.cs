using Serilog;
using Lumin.MQ.Rabbit;
using Lumin.MQ.Core;
using Lumin.MQ.Rabbit.Host;

using Lumin.MQ.Rabbit.WorkerSample;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.Async(a => a.File("Logs/log.txt"))
    .WriteTo.Debug()
    .CreateLogger();

try
{
    Log.Information("App starting. ----------------------------");

    var hostBuilder = Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostBuilderContext, services) =>
        {
            services.Configure<RabbitHubOptions>(hostBuilderContext.Configuration.GetSection(RabbitConsts.HubOptions));
            services.AddSingleton<IHubIniter, ShangHaiHubIniter>();
            services.AddRabbitService();

            services.AddHostedService<Worker>();
        })
        .ConfigureLogging((hostBuilderContext, builder) =>
        {
            builder.ConfigureSerilog(hostBuilderContext.Configuration);
        })
        .UseSerilog();

    IHost host = hostBuilder.Build();
    
    host.UseRabbitService();
    Log.Information("Rabbit inited");

    await host.RunAsync();

    Log.Information("ApplicationStopping");
    host.CloseRabbitService();
    
    Log.Information("App end. ==============================");
    Log.CloseAndFlush();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "App Crashed. =======================");
    Log.CloseAndFlush();
    return 1;
}
