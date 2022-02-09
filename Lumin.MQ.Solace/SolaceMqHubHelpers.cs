using SolaceSystems.Solclient.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Lum.MQ.Solace
{
    public partial class SolaceMqHub
    {
        private ReturnCode Pub<T>(T dto, Topic topic, string transId)
        {
            using var message = ContextFactory.Instance.CreateMessage();
            message.Destination = ContextFactory.Instance.CreateTopic(topic.Name);
            message.BinaryAttachment = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<T>(dto));
            message.CreateUserPropertyMap();
            message.UserPropertyMap.AddString(Consts.TransId, transId);

            return _session.Send(message);
        }

        private ReturnCode Send<T>(T dto, IQueue queue, string transId)
        {
            using var message = ContextFactory.Instance.CreateMessage();
            message.Destination = queue;
            message.DeliveryMode = MessageDeliveryMode.Persistent;
            message.BinaryAttachment = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));
            message.CreateUserPropertyMap();
            message.UserPropertyMap.AddString(Consts.TransId, transId);

            return _session.Send(message);
        }

        private void SendReply(object Sender, ReplyEventArgs replyEventArgs)
        {
            using (replyEventArgs.Request)
            {
                using var message = ContextFactory.Instance.CreateMessage();
                message.BinaryAttachment = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(replyEventArgs.Content));
                var returnCode = _session.SendReply(replyEventArgs.Request, message);
                if (returnCode == ReturnCode.SOLCLIENT_OK)
                {
                }
                else
                {
                }
            }
        }
        private void InitQueue(SolaceQueueOption solaceQueueOption
        , EventHandler<MessageEventArgs> messageEventHandler
            , EventHandler<FlowEventArgs> flowEventHandler
            , Dictionary<string, IFlow> queueFlowDict
            , Dictionary<string, IQueue> queueDict)
        {
            // Provision Oueue
            queueDict[solaceQueueOption.Name] = ContextFactory.Instance.CreateQueue(solaceQueueOption.Name);
            var queue = queueDict[solaceQueueOption.Name];
            int provisionFlag = ProvisionFlag.IgnoreErrorIfEndpointAlreadyExists | ProvisionFlag.WaitForConfirm;
            _session.Provision(queue, solaceQueueOption.EndpointProperties ?? SolaceQueueOption.DefaultEndpointProperties, provisionFlag, null);

            var flowProperties = solaceQueueOption.FlowProperties ?? SolaceQueueOption.DefaultFlowProperties;

            if (solaceQueueOption.MappedTopicOptions?.Any(x => x.WillSubscribeTopic) == true)
            {
                // subscribe MappedTopic
                foreach (var item in solaceQueueOption.MappedTopicOptions.Where(x => x.WillSubscribeTopic))
                {
                    var topic = ContextFactory.Instance.CreateTopic(item.Topic.Name);
                    _session.Subscribe(queue, topic, SubscribeFlag.WaitForConfirm, null);
                    // Create MappedTopicFlow
                    queueFlowDict[item.Topic.Name] = _session.CreateFlow(flowProperties, queue, null, messageEventHandler, flowEventHandler);
                }
            }
            else if (solaceQueueOption.WillSubscribeQueue)
            {
                // Create QueueFlow
                queueFlowDict[solaceQueueOption.Name] = _session.CreateFlow(flowProperties, queue, null, messageEventHandler, flowEventHandler);
            }
        }

        protected virtual IReceivedMessageDto Extract(object message)
        {
            var msg = message as IMessage;
            var result = new ReceivedMessageDto
            {
                Body = Encoding.UTF8.GetString(msg.BinaryAttachment)
            };
            if (msg.Destination is ITopic)
            {
                result.From = new Topic { Name = msg.Destination.Name };
            }
            else
            {
                result.From = new Queue { Name = msg.Destination.Name };
            }
            result.TransId = msg.UserPropertyMap?.GetString(Consts.TransId);
            return result;
        }

        struct Consts
        {
            public const string TransId = "TransId";
        }
    }
}
