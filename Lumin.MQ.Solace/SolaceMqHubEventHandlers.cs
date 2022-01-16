using Microsoft.Extensions.Logging;
using SolaceSystems.Solclient.Messaging;

namespace Lumin.MQ.Solace
{
    public partial class SolaceMqHub
    {
        private async void HandleMessage(object sender, MessageEventArgs e)
        {
            _logger.LogDebug("Solace received Handlenessage{sender}{e}", sender, e);
            var message = e.Message;
            if (_queueFlowDict.ContainsKey(message.Destination.Name))
            {
                return;
            }
            if (message.Destination is ITopic)
            {
                if (_topicRepliers.ContainsKey(message.Destination.Name))
                {
                    // NeedRgplyTopic
                    await HandleNeedReplyTopic(message);
                }
                else if (_topicSubscribers.ContainsKey(message.Destination.Name))
                {
                    // simpleTopic
                    using (message)
                        await HandleSimpleTopic(message);
                }
            }
        }

        private async void HandleQueueMessage(object sender, MessageEventArgs e)
        {
            _logger.LogDebug("Solace received HandleQueueNessage{sender}{e}", sender, e);
            using var message = e.Message;
            if (_queueFlowDict.ContainsKey(e.Message.Destination.Name))
            {
                if (message.Destination is IQueue)
                {
                    // SimpleQueue
                    if (_queueListeners.ContainsKey(message.Destination.Name))
                    {
                        await HandleSimpleQueue(message, _queueFlowDict);
                    }
                }
                else if (message.Destination is ITopic)
                {
                    // MappedTopic
                    if (_topicSubscribers.ContainsKey(message.Destination.Name))
                    {
                        await HandleMappedTopic(message, _queueFlowDict);
                    }
                }
            }
        }

        private void HandleFlowEvent(object sender, FlowEventArgs e)
        {
            _logger.LogDebug("Solace FlowEventArgs{sender}{e}", sender, e);
        }
        private void HandleSessionEvent(object sender, SessionEventArgs e)
        {
            _logger.LogDebug("Solace SessionEventArgs{sender}{e}", sender, e);
            switch (e.Event)
            {
                case SessionEvent.UpNotice:
                case SessionEvent.Reconnected:
                    _sessionUp.Set();
                    IsReady = true;
                    break;
                case SessionEvent.DownError:
                    _sessionUp.Reset();
                    IsReady = false;
                    break;
                case SessionEvent.Acknowledgement:
                case SessionEvent.RejectedMessageError:
                    OnAcked(e);
                    break;
                default:
                    break;
            }
        }

        private void OnAcked(SessionEventArgs e)
        {
            var acked = Acked;
            acked?.Invoke(this, new AckEventArgs
            {
                CorrelationKey = e.CorrelationKey,
                IsAccepted = e.Event == SessionEvent.Acknowledgement
            });
        }
    }
}