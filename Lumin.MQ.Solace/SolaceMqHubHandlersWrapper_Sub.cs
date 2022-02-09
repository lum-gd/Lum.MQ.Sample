using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lum.MQ.Solace
{
    public partial class SolaceMqHub
    {
        //Sub Queue
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
            // Suppose one queue, one body type.
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

        public void Sub<T>(Func<T, Task> action, Queue queue, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
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

        // SubTopic
        public void Sub<T>(Action<T> action, Topic topic, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            IMessageHandler messageHandler;
            bool isNew = !_topicSubscribers.ContainsKey(topic.Name);
            if (isNew)
            {
                messageHandler = _serviceProvider.GetRequiredService<IMessageHandler<T>>();
                messageHandler.MessageBox = topic;
                InitConcurrency(topic, messageHandler);
            }
            else
            {
                messageHandler = _topicSubscribers[topic.Name];
            }
            // Suppose one topic,one body type.
            if (!(messageHandler is MessageHandler<T> messageHandlerT))
            {
                throw new NotSupportedException();
            }
            messageHandlerT.AddHandler(action, who, executionDataflowBlockOptions);
            if (isNew)
            {
                _topicSubscribers[topic.Name] = messageHandlerT;
            }
        }

        public void Sub<T>(Func<T, Task> action, Topic topic, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            IMessageHandler messageHandler;
            bool isNew = !_topicSubscribers.ContainsKey(topic.Name);
            if (isNew)
            {
                messageHandler = _serviceProvider.GetRequiredService<IMessageHandler<T>>();
                messageHandler.MessageBox = topic;
                InitConcurrency(topic, messageHandler);
            }
            else
            {
                messageHandler = _topicSubscribers[topic.Name];
            }
            // Suppose one topic, one body type.
            if (!(messageHandler is MessageHandler<T> messageHandlerT))
            {
                throw new NotSupportedException();
            }
            messageHandlerT.AddHandler(action, who, executionDataflowBlockOptions);
            if (isNew)
            {
                _topicSubscribers[topic.Name] = messageHandlerT;
            }
        }

        // SubAndReply
        public void SubAndReply<TRequest, TResponse>(Func<TRequest, TResponse> func, Topic topic, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            if (!_topicRepliers.ContainsKey(topic.Name))
            {
                var topicReplier = _serviceProvider.GetRequiredService<IMessageReplier<TRequest, TResponse>>();
                topicReplier.Topic = topic;
                topicReplier.Who = who;
                topicReplier.GetReply = func;
                topicReplier.Reply += this.SendReply;
                topicReplier.InitConcurrency(executionDataflowBlockOptions);

                _topicRepliers[topic.Name] = topicReplier;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void SubAndReply<TRequest, TResponse>(Func<TRequest, Task<TResponse>> func, Topic topic, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            if (!_topicRepliers.ContainsKey(topic.Name))
            {
                var topicReplier = _serviceProvider.GetRequiredService<IMessageReplier<TRequest, TResponse>>();
                topicReplier.Topic = topic;
                topicReplier.Who = who;
                topicReplier.GetReplyAsync = func;
                topicReplier.Reply += this.SendReply;
                topicReplier.InitConcurrency(executionDataflowBlockOptions);

                _topicRepliers[topic.Name] = topicReplier;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private void InitConcurrency(IMessageBox messageBox, IMessageHandler messageHandler)
        {
            var concurrencyOption = _solaceHubOption.ConcurrencyOptions.FirstOrDefault(x => x.Name == messageBox.Name);
            var boundedCapacity = concurrencyOption != null ? concurrencyOption.BoundedCapacity : Environment.ProcessorCount;
            messageHandler.InitConcurrency(new DataflowBlockOptions { BoundedCapacity = boundedCapacity });
        }
    }
}