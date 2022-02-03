using Serilog;
using Lumin.MQ.Rabbit;
using Lumin.MQ.Core;
using Lumin.MQ.Rabbit.Host;

using Lumin.MQ.Rabbit.WorkerSample;

try
{
    var hostBuilder = Host.CreateDefaultBuilder(args)
        .ConfigureLogging((hostBuilderContext, builder) =>
        {
            builder.ConfigureSerilog(hostBuilderContext.Configuration);
        })
        .UseSerilog()
        .ConfigureServices((hostBuilderContext, services) =>
        {
            services.Configure<RabbitHubOption>(hostBuilderContext.Configuration.GetSection(RabbitConsts.HubOption));
            services.AddSingleton<IHubIniter, ShangHaiHubIniter>();
            services.AddRabbitService();

            services.AddHostedService<Worker>();
        });

    IHost host = hostBuilder.Build();
    Log.Information("App starting. ----------------------------");
    
    host.UseRabbitService();
    Log.Information("Rabbit inited");

    await host.RunAsync();

    Log.Information("ApplicationStopping");
    host.CloseRabbitService();
    Log.Information("App end. ==============================");
    
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "App Crashed. =======================");
    return 1;
}
finally{
    Log.CloseAndFlush();
}
