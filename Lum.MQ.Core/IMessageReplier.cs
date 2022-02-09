using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lum.MQ.Core
{
    public interface IMessageReplier<TMessage> : IMessageHandlerBase
    {
        Topic Topic { get; set; }
        Task<bool> HandleMessage(IReceivedMessageDto dto, TMessage requestMessage);
        string Who { get; set; }
        void InitConcurrency(ExecutionDataflowBlockOptions executionDataflowBlockOptions);
        event EventHandler<ReplyEventArgs<TMessage>> Reply;
    }

    public interface IMessageReplier<TRequest, TResponse, TMessage> : IMessageReplier<TMessage>
    {
        Func<TRequest, TResponse> GetReply { get; set; }
        Func<TRequest, Task<TResponse>> GetReplyAsync { get; set; }
    }

    public class ReplyEventArgs<TMessage>
    {
        public TMessage Request { get; set; }
        public object Content { get; set; }
    }

    public class NeedReplyItem<TMessage>
    {
        public IReceivedMessageDto ReceivedMessageDto { get; set; }
        public TMessage Request { get; set; }
    }
}
