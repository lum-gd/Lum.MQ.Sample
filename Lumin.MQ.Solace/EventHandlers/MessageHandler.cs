using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lum.MQ.Solace
{
    public class MessageHandler<T> : IMessageHandler<T>
    {
        public MessageHandler(ILogger<MessageHandler<T>> logger)
        {
            _logger = logger;
        }
        public IMessageBox MessageBox { get; set; }
        public void AddHandler(Action<T> action, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            var actionBlock = new ActionBlock<IReceivedMessageDto>(x =>
            {
                try
                {
                    var what = x.GetBodyObj<T>();
                    _logger.LogDebug("Solace parsed {who} -> {what}", who, what);
                    action(what);
                    _logger.LogDebug("Solace completed {who} -> {what}", who, what);
                    Interlocked.Increment(ref _successCount);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _errorCount);
                    _logger.LogError(ex, "Solace error {who}", who);
                }
            }, executionDataflowBlockOptions);
            _broadcastBlock.LinkTo(actionBlock);
            Whos.Add(who);
            actionBlocks.Add(actionBlock);
        }
        public void AddHandler(Func<T, Task> action, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            var actionBlock = new ActionBlock<IReceivedMessageDto>(async x =>
            {
                try
                {
                    var what = x.GetBodyObj<T>();
                    _logger.LogDebug("Solace parsed {who} -> {what}", who, what);
                    await action(what);
                    _logger.LogDebug("Solace completed (who) -> {what)", who, what);
                    Interlocked.Increment(ref _successCount);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _errorCount);
                    _logger.LogError(ex, "Solace error {who)", who);
                }
            }, executionDataflowBlockOptions);
            _broadcastBlock.LinkTo(actionBlock);
            Whos.Add(who);
            actionBlocks.Add(actionBlock);
        }

        public async Task<bool> HandleMessage(IReceivedMessageDto dto)
        {
            return await _broadcastBlock.SendAsync(dto);
        }

        public void Complete()
        {
            _broadcastBlock.Complete();
            _broadcastBlock.Completion.Wait();
            Parallel.ForEach(actionBlocks, actionBlock => { actionBlock.Complete(); actionBlock.Completion.Wait(); });
        }

        public List<string> Whos { get; private set; } = new List<string>();

        public long SuccessCount => _successCount;

        public int ProcessingCount => actionBlocks.Sum(x => x.InputCount);
        public long ErrorCount => _errorCount;

        private long _errorCount;
        private long _successCount;
        private BroadcastBlock<IReceivedMessageDto> _broadcastBlock;

        public void InitConcurrency(DataflowBlockOptions dataflowBlockOptions)
        {
            _broadcastBlock = new BroadcastBlock<IReceivedMessageDto>(x => x, dataflowBlockOptions);
        }

        private readonly ILogger _logger;
        private List<ActionBlock<IReceivedMessageDto>> actionBlocks = new List<ActionBlock<IReceivedMessageDto>>();
    }
}