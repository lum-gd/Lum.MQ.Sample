using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lumin.MQ.Core
{
    public class MessageReplier<TRequest, TResponse, TMessage> : IMessageReplier<TRequest, TResponse, TMessage>
    {
        public MessageReplier(ILogger<MessageReplier<TRequest, TRequest, TMessage>> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        public Topic Topic { get; set; }
        public void InitConcurrency(ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            _actionBlock = new ActionBlock<NeedReplyItem<TMessage>>(ReplyMessage, executionDataflowBlockOptions);
        }

        public async Task<bool> HandleMessage(IReceivedMessageDto dto, TMessage requestMessage)
        {
            return await _actionBlock.SendAsync(new NeedReplyItem<TMessage>
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

        private async Task ReplyMessage(NeedReplyItem<TMessage> needReplyItem)
        {
            IReceivedMessageDto dto = needReplyItem.ReceivedMessageDto;
            TMessage requestMessage = needReplyItem.Request;
            var id = PreProcess(dto);
            var what = dto.GetBodyObj<TRequest>();
            _logger.LogDebug("received {who}->{what}", Who, what);
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
                _logger.LogDebug("getReplied {who}->{what}->{reply}", Who, what, reply);
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
        private ActionBlock<NeedReplyItem<TMessage>> _actionBlock;

        protected virtual Guid PreProcess(IReceivedMessageDto dto)
        {
            using (var db = _serviceProvider.GetRequiredService<QDatabase>())
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
        protected virtual void PostProcess<T>(Guid id, MessageReplyResult<T> messageReplyResult, TMessage requestMessage)
        {
            OnReply(messageReplyResult, requestMessage);

            using (var db = _serviceProvider.GetRequiredService<QDatabase>())
            {
                var item = db.ReceivedDtos.FirstOrDefault(x => x.Id == id);
                if (item != null)
                {
                    item.ProcessedTime = DateTime.Now;
                    item.Processed = true;
                    item.Reply = JsonSerializer.Serialize(messageReplyResult);
                    db.SaveChanges();
                }
            }
        }

        private void OnReply(object content, TMessage requestMessage)
        {
            var reply = Reply;
            reply?.Invoke(this, new ReplyEventArgs<TMessage>
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
        public event EventHandler<ReplyEventArgs<TMessage>> Reply;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private long _errorCount;
        private long _successCount;
    }
}
