using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lumin.MQ.Solace.AspNetCore
{
    public class CheckMqHubs : IHealthCheck
    {
        public CheckMqHubs(IMqHubProvider mqHubProvider)
        {
            _mqHubProvider = mqHubProvider;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (_mqHubProvider.Hubs.Values.All(x => x.IsReady))
            {
                return Task.FromResult(HealthCheckResult.Healthy("All Ready.", _mqHubProvider.Hubs.ToDictionary(x => x.Key, x => x.Value.Statistics)));
            }
            else if (_mqHubProvider.Hubs.Values.Any(x => x.IsReady))
            {
                return Task.FromResult(HealthCheckResult.Degraded("Not All Ready.", null, _mqHubProvider.Hubs.ToDictionary(x => x.Key, x => x.Value.Statistics)));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("All Not Ready.", null, _mqHubProvider.Hubs.ToDictionary(x => x.Key, x => x.Value.Statistics)));
            }
        }

        private readonly IMqHubProvider _mqHubProvider;
    }

    public class CheckMqHub
    {
        public CheckMqHub(IMqHub mqHub)
        {
            _mqHub = mqHub;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (_mqHub.IsReady)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Ready.", new Dictionary<string, object> { { _mqHub.HubName, _mqHub.Statistics } }));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Not Ready.", null, new Dictionary<string, object> { { _mqHub.HubName, _mqHub.Statistics } }));
            }
        }

        private readonly IMqHub _mqHub;
    }
}