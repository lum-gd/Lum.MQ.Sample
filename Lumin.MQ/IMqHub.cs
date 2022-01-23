using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lumin.MQ
{
    public interface IMqHub : IDisposable
    {
        string HubName { get; }

        void Sub<T>(Action<T> action, Queue queue, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions);
        void Sub<T>(Action<T> action, Topic topic, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions);
        void SubAndReply<TRequest, TResponse>(Func<TRequest, TResponse> func, Topic topic, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions);

        void Sub<T>(Func<T, Task> action, Queue queue, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions);
        void Sub<T>(Func<T, Task> action, Topic topic, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions);
        void SubAndReply<TRequest, TResponse>(Func<TRequest, Task<TResponse>> func, Topic topic, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions);

        PubResponse Pub<T>(T dto, Queue queue, string who, string transId = null);
        PubResponse Pub<T>(T dto, Topic topic, string who, string transId = null);
        ReqResponse<MessageReplyResult<TResponse>> Req<TRequest, TResponse>(TRequest dto, Topic topic, TimeSpan tineOut, string who, string transId = null);

        void Start();

        bool IsReady { get; }
        object Statistics { get; }
        long SentOkCount { get; }
        long SentFailCount { get; }
        long ReceivedOkCount { get; }
        long ReceivedFailCount { get; }
        event EventHandler<PubFailArgs> PubFail;
        event EventHandler<AckEventArgs> Acked;
    }
}