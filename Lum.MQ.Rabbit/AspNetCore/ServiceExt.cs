using Lum.MQ.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lum.MQ.Rabbit.AspNetCore
{
    public static class ServiceExt
    {
        public static IApplicationBuilder UseRabbitService(this IApplicationBuilder app, IHostApplicationLifetime hostApplicationLifetime)
        {
            var qDatabase = app.ApplicationServices.GetRequiredService<QDatabase>();
            qDatabase.Database.EnsureCreated();

            var mqHub = app.ApplicationServices.GetRequiredService<IMqHub>();
            var hubIniter = app.ApplicationServices.GetService<IHubIniter>();
            if (hubIniter != null)
            {
                hubIniter.SubQueue(mqHub);
                hubIniter.SubTopic(mqHub);
            }

            mqHub.Start();

            hostApplicationLifetime.ApplicationStopping.Register(() =>
            {
                mqHub.Dispose();
            });

            return app;
        }
    }
}
