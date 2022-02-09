using Microsoft.Extensions.DependencyInjection;
using SolaceSystems.Solclient.Messaging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lum.MQ.Solace
{
    public partial class SolaceMqHub
    {
        private async Task HandleSimpleQueue(IMessage message, Dictionary<string, IFlow> queueFlowDict)
        {
            var adMessageId = message.ADMessageId;
            var dto = Extract(message);
            if (await _queueListeners[dto.From.Name].HandleMessage(dto))
            {
                queueFlowDict[dto.From.Name].Ack(adMessageId);
            }
        }
        private async Task HandleMappedTopic(IMessage message, Dictionary<string, IFlow> queueFlowDict)
        {
            var adMessageId = message.ADMessageId;
            IReceivedMessageDto dto = Extract(message);
            if (await _topicSubscribers[dto.From.Name].HandleMessage(dto))
            {
                _queueFlowDict[dto.From.Name].Ack(adMessageId);
            }
        }

        private async Task HandleSimpleTopic(IMessage message)
        {
            IReceivedMessageDto dto = Extract(message);
            await _topicSubscribers[dto.From.Name].HandleMessage(dto);
        }
        private async Task HandleNeedReplyTopic(IMessage message)
        {
            IReceivedMessageDto dto = Extract(message);
            await _topicRepliers[message.Destination.Name].HandleMessage(dto, message);
        }
        public object Statistics
        {
            get
            {

                int messagesCount;
                List<ReceivedDto> last3Messages;
                using (var db = _serviceProvider.GetRequiredService<SolaceDatabase>())
                {
                    last3Messages = db.ReceivedDtos.OrderByDescending(x => x.ReceivedTime).Take(3).ToList();
                    messagesCount = db.ReceivedDtos.Count();
                    return new
                    {
                        messagesCount,
                        last3Messages,
                        HandlerStatistics
                    };
                }
            }
        }

        private readonly Dictionary<string, IMessageHandler> _queueListeners = new Dictionary<string, IMessageHandler>();
        private readonly Dictionary<string, IMessageHandler> _topicSubscribers = new Dictionary<string, IMessageHandler>();
        private readonly Dictionary<string, IMessageReplier> _topicRepliers = new Dictionary<string, IMessageReplier>();
        object HandlerStatistics
        {
            get
            {
                return new
                {
                    SentOkCount,
                    SentFailCount,
                    ReceivedOkCount,
                    ReceivedFailCount,
                    QueueListeners = _queueListeners.Values.Select(x => new { x.Whos, x.ProcessingCount, x.SuccessCount, x.ErrorCount }),
                    TopicSubscribers = _topicSubscribers.Values.Select(x => new { x.Whos, x.ProcessingCount, x.SuccessCount, x.ErrorCount }),
                    TopicRepliers = _topicRepliers.Values.Select(x => new { x.Who, x.SuccessCount, x.ErrorCount })
                };
            }
        }
        public class MessageReplyResult
        {
            public object Result { get; set; }
            public string ErrorMsg { get; set; }
        }
    }
}
