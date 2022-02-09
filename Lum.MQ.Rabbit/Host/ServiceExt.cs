using Microsoft.Extensions.DependencyInjection;
using Lum.MQ.Core;
using Microsoft.Extensions.Hosting;

namespace Lum.MQ.Rabbit.Host
{
    public static class ServiceExt
    {
        public static IServiceCollection AddRabbitService(this IServiceCollection services)
        {
            services.AddTransient<QDatabase>();
            services.AddEntityFrameworkSqlite().AddDbContext<QDatabase>();

            services.AddSingleton<IMqHub, RabbitMqHub>();
            services.AddTransient(typeof(IMessageHandler<>), typeof(MessageHandler<>));
            services.AddTransient(typeof(IMessageReplier<,,>), typeof(MessageReplier<,,>));
            return services;
        }

        public static IHost UseRabbitService(this IHost host)
        {
            var qDatabase = host.Services.GetRequiredService<QDatabase>();
            qDatabase.Database.EnsureCreated();

            var mqHub = host.Services.GetRequiredService<IMqHub>();
            var hubIniter = host.Services.GetService<IHubIniter>();
            if (hubIniter != null)
            {
                hubIniter.SubQueue(mqHub);
                hubIniter.SubTopic(mqHub);
            }

            mqHub.Start();

            return host;
        }

        public static void CloseRabbitService(this IHost host)
        {
            var mqHub = host.Services.GetRequiredService<IMqHub>();
            mqHub?.Dispose();
        }
    }
}