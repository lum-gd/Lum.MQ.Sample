using Serilog;
using Lumin.MQ.Rabbit.AspNetCore;
using Lumin.MQ.Rabbit;
using Lumin.MQ.Core;
using Lumin.MQ.Rabbit.Host;

using Lumin.MQ.Rabbit.WebSample;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.Async(a => a.File("Logs/log.txt"))
    .WriteTo.Debug()
    .CreateLogger();

try
{
    Log.Information("App starting. ----------------------------");

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.Configure<RabbitHubOptions>(builder.Configuration.GetSection(RabbitConsts.HubOptions));
    builder.Services.AddRabbitService();

    builder.Services.AddRazorPages();
    builder.Host.UseSerilog();
    
    var app = builder.Build();

    var hostApplicationLifetime = app.Lifetime;
    Console.CancelKeyPress += (sender, eventArgs) =>
    {
        Log.Information("CacelKeyPress");
        hostApplicationLifetime.StopApplication();
        eventArgs.Cancel = true;
    };

    app.UseRabbitService(hostApplicationLifetime);
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
