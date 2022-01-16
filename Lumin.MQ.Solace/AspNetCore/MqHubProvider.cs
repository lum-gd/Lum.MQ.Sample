using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolaceSystems.Solclient.Messaging;
using System;
using System.Collections.Generic;

namespace Lumin.MQ.Solace.AspNetCore
{
    public class MqHubProvider : IMqHubProvider
    {
        public MqHubProvider(IOptions<SolaceHubOptions> solaceHubOptions, IContext context, IEnumerable<IHubIniter> initers, IServiceProvider serviceProvider, ILogger<MqHubProvider> logger)
        {
            _solaceHubOptions = solaceHubOptions.Value;
            _context = context;
            _logger = logger;
            _initers = initers;
            _serviceProvider = serviceProvider;

            Init();
        }
        private void Init()
        {
            foreach (var solaceOption in _solaceHubOptions.Options)
                {
                _hubs[solaceOption.HubName] = new SolaceMqHub(_context, Options.Create(solaceOption), _serviceProvider.GetRequiredService<ILogger<SolaceMqHub>>(), _serviceProvider);
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

        private readonly IContext _context;
        private readonly SolaceHubOptions _solaceHubOptions;

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
                foreach (var solaceOption in _solaceHubOptions.Options)
                {
                    _hubs[solaceOption.HubName].Dispose();
                }

                _context.Dispose();

                _logger.LogInformation("MqHubProvider Disposed");
            }

            _isDisposed = true;
        }

        private bool _isDisposed = false;
        #endregion
    }
}