using Lumin.MQ.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumin.MQ.Rabbit
{
    public class MqHubProvider : IMqHubProvider
    {
        public MqHubProvider(IOptions<RabbitHubOptions> rabbitHubOptions, IEnumerable<IHubIniter> initers, IServiceProvider serviceProvider, ILogger<MqHubProvider> logger)
        {
            _rabbitHubOptions = rabbitHubOptions.Value;
            _logger = logger;
            _initers = initers;
            _serviceProvider = serviceProvider;

            Init();
        }
        private void Init()
        {
            foreach (var rabbitOption in _rabbitHubOptions.Options)
            {
                _hubs[rabbitOption.HubName] = new RabbitMqHub(rabbitOption, _serviceProvider.GetRequiredService<ILogger<RabbitMqHub>>(), _serviceProvider);
            }

            if (_initers != null)
            {
                foreach (var initer in _initers)
                {
                    var hub = _hubs[initer.HubName];
                    initer.SubQueue(hub);
                    initer.SubTopic(hub);
                }
            }

            _logger.LogInformation("");
        }

        public IReadOnlyDictionary<string, IMqHub> Hubs => _hubs;

        private readonly RabbitHubOptions _rabbitHubOptions;

        private readonly Dictionary<string, IMqHub> _hubs = new Dictionary<string, IMqHub>();
        private readonly ILogger _logger;
        private readonly IEnumerable<IHubIniter> _initers;
        private readonly IServiceProvider _serviceProvider;

        #region dispose
        public void Dispose()
        {
            if (!_isDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
        ~MqHubProvider()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {

            if (_isDisposed) return;
            if (disposing)
            {
                foreach (var rabbitOption in _rabbitHubOptions.Options)
                {
                    _hubs[rabbitOption.HubName].Dispose();
                }

                _logger.LogInformation("MqHubProvider Disposed");
            }

            _isDisposed = true;
        }

        private bool _isDisposed = false;
        #endregion
    }
}
