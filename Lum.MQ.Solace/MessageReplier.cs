using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolaceSystems.Solclient.Messaging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lum.MQ.Solace
{
    public class MessageReplier<TRequest, TResponse> : IMessageReplier<TRequest, TResponse>
    {
        public MessageReplier(ILogger<MessageReplier<TRequest, TRequest>> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        public Topic Topic { get; set; }
        public void InitConcurrency(ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            _actionBlock = new ActionBlock<NeedReplyItem>(ReplyMessage, executionDataflowBlockOptions);
        }

        public async Task<bool> HandleMessage(IReceivedMessageDto dto, IMessage requestMessage)
        {
            return await _actionBlock.SendAsync(new NeedReplyItem
            {
                ReceivedMessageDto = dto,
                Request = requestMessage
            });
        }

        public void Complete()
        {
            _actionBlock.Complete();
            _actionBlock.Completion.Wait();
        }

        private async Task ReplyMessage(NeedReplyItem needReplyItem)
        {
            IReceivedMessageDto dto = needReplyItem.ReceivedMessageDto;
            IMessage requestMessage = needReplyItem.Request;
            var id = PreProcess(dto);
            var what = dto.GetBodyObj<TRequest>();
            _logger.LogDebug("Solace received {who}->{what}", Who, what);
            MessageReplyResult<TResponse> responseDto;
            try
            {
                TResponse reply;
                if (GetReplyAsync != null)
                {
                    reply = await GetReplyAsync(what);
                }
                else
                {
                    reply = GetReply(what);
                }
                _logger.LogDebug("Solace getReplied {who}->{what}->{reply}", Who, what, reply);
                responseDto = new MessageReplyResult<TResponse>
                {
                    Result = reply
                };

                Interlocked.Increment(ref _successCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReplyMessage Error");
                responseDto = new MessageReplyResult<TResponse>
                {
                    ErrorMsg = ex.Message
                };
                Interlocked.Increment(ref _errorCount);
            }
            PostProcess(id, responseDto, requestMessage);
        }
        private ActionBlock<NeedReplyItem> _actionBlock;

        protected virtual Guid PreProcess(IReceivedMessageDto dto)
        {
            using (var db = _serviceProvider.GetRequiredService<SolaceDatabase>())
            {
                var item = new ReceivedDto
                {
                    Id = Guid.NewGuid(),
                    Body = dto.Body,
                    SourceType = "Topic",
                    SourceName = dto.From.Name,
                    ReceivedTime = DateTime.Now,
                    TransId = dto.TransId,
                };
                db.ReceivedDtos.Add(item);
                db.SaveChanges();
                return item.Id;
            }
        }
        protected virtual void PostProcess<T>(Guid id, MessageReplyResult<T> messageReplyResult, IMessage requestMessage)
        {
            OnReply(messageReplyResult, requestMessage);

            using (var db = _serviceProvider.GetRequiredService<SolaceDatabase>())
            {
                var item = db.ReceivedDtos.FirstOrDefault(x => x.Id == id);
                item.ProcessedTime = DateTime.Now;
                item.Processed = true;
                item.Reply = JsonSerializer.Serialize(messageReplyResult);
                db.SaveChanges();
            }
        }

        private void OnReply(object content, IMessage requestMessage)
        {
            var reply = Reply;
            reply?.Invoke(this, new ReplyEventArgs
            {
                Content = content,
                Request = requestMessage
            });
        }

        public string Who { get; set; }
        public long SuccessCount => _successCount;
        public long ErrorCount => _errorCount;
        public Func<TRequest, TResponse> GetReply { get; set; }
        public Func<TRequest, Task<TResponse>> GetReplyAsync { get; set; }
        public event EventHandler<ReplyEventArgs> Reply;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private long _errorCount;
        private long _successCount;
    }
}