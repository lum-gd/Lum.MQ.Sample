using SolaceSystems.Solclient.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lum.MQ.Solace
{
    public interface IMessageReplier : IMessageHandlerBase
    {
        Topic Topic { get; set; }
        Task<bool> HandleMessage(IReceivedMessageDto dto, IMessage requestMessage);
        string Who { get; set; }
        void InitConcurrency(ExecutionDataflowBlockOptions executionDataflowBlockOptions);
        event EventHandler<ReplyEventArgs> Reply;
    }

    public interface IMessageReplier<TRequest, TResponse> : IMessageReplier
    {
        Func<TRequest, TResponse> GetReply { get; set; }
        Func<TRequest, Task<TResponse>> GetReplyAsync { get; set; }
    }

    public class ReplyEventArgs
    {
        public IMessage Request { get; set; }
        public object Content { get; set; }
    }

    public class NeedReplyItem
    {
        public IReceivedMessageDto ReceivedMessageDto { get; set; }
        public IMessage Request { get; set; }
    }
}
