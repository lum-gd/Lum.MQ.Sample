using Lumin.MQ.Core;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Events;

namespace Lumin.MQ.Rabbit
{
    public class RabbitMqHub : IMqHub, IQueueCollection, ITopicCollection
    {
        public RabbitMqHub(RabbitHubOption rabbitHubOption, ILogger<RabbitMqHub> logger, IServiceProvider serviceProvider)
        {
            _rabbitHubOption = rabbitHubOption;
            _connectionFactory = new ConnectionFactory() { HostName = rabbitHubOption.Host };
            _connection = _connectionFactory.CreateConnection();
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public IReadOnlyCollection<Queue> Queues => _queueListeners.Values.Select(x => x.MessageBox as Queue).ToList();

        public IReadOnlyCollection<Topic> Topics => _topicSubscribers.Values.Select(x => x.MessageBox as Topic).ToList();

        public IReadOnlyCollection<Topic> TopicsNeedReply => _topicRepliers.Values.Select(x => x.Topic).ToList();

        private RabbitHubOption _rabbitHubOption;
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private ILogger _logger;
        private IServiceProvider _serviceProvider;
        private readonly Dictionary<string, IModel> _queueDict = new Dictionary<string, IModel>();

        public bool IsReady => throw new NotImplementedException();

        public object Statistics => throw new NotImplementedException();

        public event EventHandler<PubFailArgs>? PubFail;
        public event EventHandler<AckEventArgs>? Acked;

        public string HubName { get { return _rabbitHubOption.HubName; } }

        public long SentOkCount => _sentOkCount;
        private long _sentOkCount;
        public long SentFailCount => _sentFailCount;
        private long _sentFailCount;
        private readonly Dictionary<string, IMessageHandler> _queueListeners = new Dictionary<string, IMessageHandler>();
        private readonly Dictionary<string, IMessageHandler> _topicSubscribers = new Dictionary<string, IMessageHandler>();
        private readonly Dictionary<string, IMessageReplier<string>> _topicRepliers = new Dictionary<string, IMessageReplier<string>>();
        private readonly Dictionary<string, EventingBasicConsumer> _queueConsumer = new Dictionary<string, EventingBasicConsumer>();

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

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        ~RabbitMqHub()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                Complete();
                foreach (var item in _queueDict?.Values)
                {
                    item.Dispose();
                }
                _connection.Dispose();

                _logger.LogInformation("Rabbit MqHub {HubNane} statistics: {Statistics}", HubName,
                    JsonSerializer.Serialize(Statistics, new JsonSerializerOptions { WriteIndented = true }));
                _logger.LogInformation("Rabbit MqHub {HubNane} Disposed", HubName);
                _isDisposed = true;
            }
        }

        private void Complete()
        {
            //foreach (var topic in _rabbitHubOption.Topics)
            //{
            //    _session.Unsubscribe(ContextFactory.Instance.CreateTopic(topic), true);
            //}
            //foreach (var topicSubscriber in _topicSubscribers.Values)
            //{
            //    _session.Unsubscribe(ContextFactory.Instance.CreateTopic(topicSubscriber.MessageBox.Name), true);
            //}

            //foreach (var queueFlow in _queueFlowDict.Values)
            //{
            //    queueFlow.Stop();
            //}
            List<IMessageHandlerBase> messageHandlers = new List<IMessageHandlerBase>();
            messageHandlers.AddRange(_queueListeners?.Values);
            messageHandlers.AddRange(_topicSubscribers?.Values);
            messageHandlers.AddRange(_topicRepliers?.Values);
            Parallel.ForEach(messageHandlers, messageHandler => messageHandler.Complete());
        }

        private bool _isDisposed = false;

        public void Start()
        {
            foreach (var rabbitQueueOption in _rabbitHubOption.RabbitQueueOptions)
            {
                InitQueue(rabbitQueueOption, _queueDict);
            }

            _logger.LogInformation("Rabbit MqHub {hubName} started", HubName);
        }

        private void InitQueue(RabbitQueueOption rabbitQueueOption, Dictionary<string, IModel> queueDict)
        {
            // Prepare Oueue
            queueDict[rabbitQueueOption.Name] = _connection.CreateModel();
            var channel = queueDict[rabbitQueueOption.Name];
            channel.QueueDeclare(queue: rabbitQueueOption.Name,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // Listen Q
            if (rabbitQueueOption.WillSubscribeQueue)
            {
                _queueConsumer[rabbitQueueOption.Name] = new EventingBasicConsumer(channel);
                var consumer = _queueConsumer[rabbitQueueOption.Name];
                consumer.Received += async (sender, e) =>
                {
                    _logger.LogDebug("Rabbit received HandleQueueNessage{sender}{e}", sender, e);
                    var body = e.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    if (_queueListeners.ContainsKey(rabbitQueueOption.Name))
                    {
                        await HandleSimpleQueue(message, rabbitQueueOption.Name, e.BasicProperties.CorrelationId);
                    }
                };
                channel.BasicConsume(queue: rabbitQueueOption.Name,
                                     autoAck: true,
                                     consumer: consumer);
            }
        }

        private async Task HandleSimpleQueue(string message, string qName, string transId)
        {
            var dto = new ReceivedMessageDto
            {
                Body = message,
                From = new Queue { Name = qName },
                TransId = transId
            };
            await _queueListeners[qName].HandleMessage(dto);
        }

        public PubResponse Pub<T>(T dto, Queue queue, string who, string transId = null)
        {
            _logger.LogDebug("Rabbit send {who}->{dto}->{queue}", who, dto, queue);
            var q = _queueDict[queue.Name];
            try
            {
                Send(dto, q, transId);
                return GenPubResponse(null, queue, who, dto);
            }
            catch (Exception ex)
            {
                return GenPubResponse(ex, queue, who, dto);
            }
        }

        private void Send<T>(T dto, IModel queue, string transId)
        {
            var basicProperties = queue.CreateBasicProperties();
            basicProperties.CorrelationId = transId;
            basicProperties.AppId = HubName;
            queue.BasicPublish(exchange: "",
                                 routingKey: "hello",
                                 basicProperties,
                                 body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto)));
        }

        private PubResponse GenPubResponse(Exception ex, IMessageBox where, string who, object what)
        {
            if (ex == null)
            {
                Interlocked.Increment(ref _sentOkCount);
                return new PubResponse
                {
                    IsSuccess = true,
                };
            }
            else
            {
                Interlocked.Increment(ref _sentFailCount);
                OnPubFail(ex, where, who, what);
                return new PubResponse
                {
                    IsSuccess = false,
                    ErrorMsg = ex.Message
                };
            }
        }

        private void OnPubFail(Exception ex, IMessageBox where, string who, object what)
        {
            var pubFail = this.PubFail;
            pubFail?.Invoke(this, new PubFailArgs
            {
                Who = who,
                Where = where,
                ErrorMsg = ex.Message,
                What = what
            });
        }

        public PubResponse Pub<T>(T dto, Topic topic, string who, string transId = null)
        {
            throw new NotImplementedException();
        }

        public ReqResponse<MessageReplyResult<TResponse>> Req<TRequest, TResponse>(TRequest dto, Topic topic, TimeSpan tineOut, string who, string transId = null)
        {
            throw new NotImplementedException();
        }

        public void Sub<T>(Action<T> action, Queue queue, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            IMessageHandler messageHandler;
            bool isNew = !_queueListeners.TryGetValue(queue.Name, out messageHandler);
            if (isNew)
            {
                messageHandler = _serviceProvider.GetRequiredService<IMessageHandler<T>>();
                messageHandler.MessageBox = queue;
                InitConcurrency(queue, messageHandler);
            }
            //Suppose one queue,one body type.
            if (!(messageHandler is MessageHandler<T> messageHandlerT))
            {
                throw new NotSupportedException();
            }
            messageHandlerT.AddHandler(action, who, executionDataflowBlockOptions);
            if (isNew)
            {
                _queueListeners[queue.Name] = messageHandlerT;
            }
        }

        private void InitConcurrency(IMessageBox messageBox, IMessageHandler messageHandler)
        {
            var concurrencyOption = _rabbitHubOption.ConcurrencyOptions.FirstOrDefault(x => x.Name == messageBox.Name);
            var boundedCapacity = concurrencyOption != null ? concurrencyOption.BoundedCapacity : Environment.ProcessorCount;
            messageHandler.InitConcurrency(new DataflowBlockOptions { BoundedCapacity = boundedCapacity });
        }

        public void Sub<T>(Action<T> action, Topic topic, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            throw new NotImplementedException();
        }

        public void Sub<T>(Func<T, Task> action, Queue queue, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            throw new NotImplementedException();
        }

        public void Sub<T>(Func<T, Task> action, Topic topic, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            throw new NotImplementedException();
        }

        public void SubAndReply<TRequest, TResponse>(Func<TRequest, TResponse> func, Topic topic, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            throw new NotImplementedException();
        }

        public void SubAndReply<TRequest, TResponse>(Func<TRequest, Task<TResponse>> func, Topic topic, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            throw new NotImplementedException();
        }
    }
}