using System.Threading.Tasks.Dataflow;

namespace Lumin.MQ.Core
{
    public interface IMessageHandler : IMessageHandlerBase
    {
        IMessageBox MessageBox { get; set; }
        Task<bool> HandleMessage(IReceivedMessageDto dto);
        List<string> Whos { get; }
        int ProcessingCount { get; }
        void InitConcurrency(DataflowBlockOptions dataflowBlockOptions);
    }

    public interface IMessageHandler<T> : IMessageHandler
    {
        void AddHandler(Action<T> action, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions);
        void AddHandler(Func<T, Task> action, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions);
    }

    public interface IMessageHandlerBase
    {
        void Complete();
        long SuccessCount { get; }
        long ErrorCount { get; }
    }
}