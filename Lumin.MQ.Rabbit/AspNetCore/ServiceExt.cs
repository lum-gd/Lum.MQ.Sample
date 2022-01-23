using Lumin.MQ.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumin.MQ.Rabbit.AspNetCore
{
    public static class ServiceExt
    {
        public static IServiceCollection AddDefaultRabbitServices(this IServiceCollection services)
        {
            return services.AddRabbitServices();
        }

        public static IServiceCollection AddRabbitServices(this IServiceCollection services)
        {
            services.AddTransient<QDatabase>();
            services.AddEntityFrameworkSqlite().AddDbContext<QDatabase>();

            services.AddSingleton<IMqHubProvider, MqHubProvider>();
            services.AddTransient(typeof(IMessageHandler<>), typeof(MessageHandler<>));
            services.AddTransient(typeof(IMessageReplier<,,>), typeof(MessageReplier<,,>));

            return services;
        }

        public static IApplicationBuilder UseRabbitServices(this IApplicationBuilder app, IHostApplicationLifetime hostApplicationLifetime)
        {
            var qDatabase = app.ApplicationServices.GetRequiredService<QDatabase>();
            qDatabase.Database.EnsureCreated();

            var mqHubProvider = app.ApplicationServices.GetRequiredService<IMqHubProvider>();
            foreach (var hub in mqHubProvider.Hubs.Values)
            {
                hub.Start();
            }

            hostApplicationLifetime.ApplicationStopping.Register(() =>
            {
                mqHubProvider.Dispose();
            });

            return app;
        }

        public static IServiceCollection AddRabbitService(this IServiceCollection services)
        {
            services.AddTransient<QDatabase>();
            services.AddEntityFrameworkSqlite().AddDbContext<QDatabase>();

            services.AddSingleton<IMqHub, RabbitMqHub>();
            services.AddTransient(typeof(IMessageHandler<>), typeof(MessageHandler<>));
            services.AddTransient(typeof(IMessageReplier<,,>), typeof(MessageReplier<,,>));
            return services;
        }

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
