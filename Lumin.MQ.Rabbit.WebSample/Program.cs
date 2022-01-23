using Serilog;
using Lumin.MQ.Rabbit.AspNetCore;
using Lumin.MQ.Rabbit;
using Lumin.MQ.Rabbit.WebSample;
using Lumin.MQ.Core;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.Async(a => a.File("Logs/log.txt"))
    .WriteTo.Debug()
    //.WriteTo.SQLite("Logs/log.db".CreateLogger();
    .CreateLogger();

try
{
    Log.Information("App starting. ----------------------------");

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.Configure<RabbitHubOptions>(builder.Configuration.GetSection(Consts.RabbitHubOptions));
    builder.Services.AddSingleton<ShangHaiHubIniter>();
    builder.Services.AddDefaultRabbitServices();

    builder.Services.AddRazorPages();
    builder.Host.UseSerilog();
    
    var app = builder.Build();

    var hostApplicationLifetime = app.Lifetime;
    hostApplicationLifetime.ApplicationStarted.Register(() =>
    {
        var mqHubProvider = app.Services.GetRequiredService<IMqHubProvider>();
        var shangHaiHubIniter = app.Services.GetRequiredService<ShangHaiHubIniter>();
        shangHaiHubIniter.SubQueue(mqHubProvider.Hubs[MyHubs.ShangHai]);
        shangHaiHubIniter.SubTopic(mqHubProvider.Hubs[MyHubs.ShangHai]);

        Log.Information("ApplicationStarted");
    });
    hostApplicationLifetime.ApplicationStopping.Register(() =>
    {
        Log.Information("ApplicationStopping");
        var mqHubProvider = app.Services.GetRequiredService<IMqHubProvider>();
        mqHubProvider.Dispose();
    });
    hostApplicationLifetime.ApplicationStopped.Register(() => Log.Information("ApplicationStopped"));
    Console.CancelKeyPress += (sender, eventArgs) =>
    {
        Log.Information("CacelKeyPress");
        hostApplicationLifetime.StopApplication();
        eventArgs.Cancel = true;
    };

    app.UseRabbitServices(hostApplicationLifetime);
    Log.Information("Rabbit inited");

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.MapRazorPages();

    app.Run();

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
