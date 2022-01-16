using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolaceSystems.Solclient.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Lumin.MQ.Solace
{
    public partial class SolaceMqHub : IMqHub, IQueueCollection, ITopicCollection
    {
        public SolaceMqHub(IContext context, IOptions<SolaceHubOption> solaceHubOptions, ILogger<SolaceMqHub> logger, IServiceProvider serviceProvider)
        {
            _context = context;
            _solaceHubOption = solaceHubOptions.Value;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public void Start()
        {
            _session = _context.CreateSession(_solaceHubOption.SessionProperties, HandleMessage, HandleSessionEvent);
            _session.Connect();
            foreach (var solaceQueueOption in _solaceHubOption.SolaceQueueOptions)
            {
                InitQueue(solaceQueueOption, HandleQueueMessage, HandleFlowEvent, _queueFlowDict, _queueDict);
            }
            foreach (var topic in _solaceHubOption.Topics)
            {
                _session.Subscribe(ContextFactory.Instance.CreateTopic(topic), true);
            }
            foreach (var flow in _queueFlowDict.Values)
            {
                flow.Start();
            }
            _logger.LogInformation("Solace MqHub {hubName} started", HubName);
        }

        public event EventHandler<PubFailArgs> PubFail;
        public event EventHandler<AckEventArgs> Acked;

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        ~SolaceMqHub()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                Complete();
                _session?.Dispose();
                foreach (var item in _queueDict?.Values)
                {
                    item.Dispose();
                }
                foreach (var item in _queueFlowDict?.Values)
                {
                    item.Dispose();
                }
                _sessionUp.Dispose();
                _logger.LogInformation("Solace MqHub {HubNane} statistics: {Statistics}", HubName,
                    JsonSerializer.Serialize(Statistics, new JsonSerializerOptions { WriteIndented = true }));
                _logger.LogInformation("Solace MqHub {HubNane} Disposed", HubName);
                _isDisposed = true;
            }
        }

        private void Complete()
        {
            foreach (var topic in _solaceHubOption.Topics)
            {
                _session.Unsubscribe(ContextFactory.Instance.CreateTopic(topic), true);
            }
            foreach (var topicSubscriber in _topicSubscribers.Values)
            {
                _session.Unsubscribe(ContextFactory.Instance.CreateTopic(topicSubscriber.MessageBox.Name), true);
            }

            foreach (var queueFlow in _queueFlowDict.Values)
            {
                queueFlow.Stop();
            }
            List<IMessageHandlerBase> messageHandlers = new List<IMessageHandlerBase>();
            messageHandlers.AddRange(_queueListeners?.Values);
            messageHandlers.AddRange(_topicSubscribers?.Values);
            messageHandlers.AddRange(_topicRepliers?.Values);
            Parallel.ForEach(messageHandlers, messageHandler => messageHandler.Complete());
        }

        private bool _isDisposed = false;
        public string HubName { get { return _solaceHubOption.HubName; } }

        public long SentOkCount => _sentOkCount;
        private long _sentOkCount;
        public long SentFailCount => _sentFailCount;
        private long _sentFailCount;

        public long ReceivedOkCount
        {
            get
            {
                return _queueListeners?.Values.Select(x => x.SuccessCount).Sum() +
                                    _topicSubscribers?.Values.Select(x => x.SuccessCount).Sum() +
                                    _topicRepliers?.Values.Select(x => x.SuccessCount).Sum() ?? 0;
            }
        }

        public long ReceivedFailCount
        {
            get
            {
                return _queueListeners?.Values.Select(x => x.ErrorCount).Sum() +
                        _topicSubscribers?.Values.Select(x => x.ErrorCount).Sum() +
                        _topicRepliers?.Values.Select(x => x.ErrorCount).Sum() ?? 0;
            }
        }

        private readonly SolaceHubOption _solaceHubOption;
        IContext _context;
        ILogger _logger;
        IServiceProvider _serviceProvider;
        private ISession _session;
        private readonly Dictionary<string, IFlow> _queueFlowDict = new Dictionary<string, IFlow>();
        private readonly Dictionary<string, IQueue> _queueDict = new Dictionary<string, IQueue>();
        private ManualResetEvent _sessionUp = new ManualResetEvent(false);
        public bool IsReady { get; private set; }
    }
}