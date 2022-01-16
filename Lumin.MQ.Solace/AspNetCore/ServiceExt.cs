using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolaceSystems.Solclient.Messaging;

namespace Lumin.MQ.Solace.AspNetCore
{
    public static class ServiceExt
    {
        public static IServiceCollection AddDefaultSolaceServices(this IServiceCollection services)
        {
            return services.AddSolaceServices(ContextInstance.SolaceContextInstance);
        }

        public static IServiceCollection AddSolaceServices(this IServiceCollection services, IContext context)
        {
            services.AddTransient<SolaceDatabase>();
            services.AddEntityFrameworkSqlite().AddDbContext<SolaceDatabase>();

            services.AddSingleton(context);
            services.AddSingleton<IMqHubProvider, MqHubProvider>();
            services.AddTransient(typeof(IMessageHandler<>), typeof(MessageHandler<>));
            services.AddTransient(typeof(IMessageReplier<,>), typeof(IMessageReplier<,>));

            return services;
        }

        public static IApplicationBuilder UseSolaceServices(this IApplicationBuilder app, IHostApplicationLifetime hostApplicationLifetime)
        {
            var solaceDatabase = app.ApplicationServices.GetRequiredService<SolaceDatabase>();
            solaceDatabase.Database.EnsureCreated();

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


        public static IServiceCollection AddDefautsSolavice(this IServiceCollection services)
        {
            return services.AddSolaceService(ContextInstance.SolaceContextInstance);
        }

        public static IServiceCollection AddSolaceService(this IServiceCollection services, IContext context)
        {
            services.AddTransient<SolaceDatabase>();
            services.AddEntityFrameworkSqlite().AddDbContext<SolaceDatabase>();

            services.AddSingleton(context);
            services.AddSingleton<IMqHub, SolaceMqHub>();
            services.AddTransient(typeof(IMessageHandler<>), typeof(MessageHandler<>));
            services.AddTransient(typeof(IMessageReplier<,>), typeof(MessageReplier<,>));
            return services;
        }

        public static IApplicationBuilder UseSolaceService(this IApplicationBuilder app, IHostApplicationLifetime hostApplicationLifetime)
        {
            var solaceDatabase = app.ApplicationServices.GetRequiredService<SolaceDatabase>();
            solaceDatabase.Database.EnsureCreated();

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