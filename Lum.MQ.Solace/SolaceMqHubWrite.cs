using Microsoft.Extensions.Logging;
using SolaceSystems.Solclient.Messaging;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Lum.MQ.Solace
{
    public partial class SolaceMqHub
    {
        public PubResponse Pub<T>(T dto, Queue queue, string who, string transId)
        {
            _sessionUp.WaitOne();
            _logger.LogDebug("Solace send {who}->{dto}->{queue}", who, dto, queue);
            var q = _queueDict[queue.Name];
            var returnCode = Send(dto, q, transId);
            return GenPubResponse(returnCode, queue, who, dto);
        }

        public PubResponse Pub<T>(T dto, Topic topic, string who, string transId)
        {
            _sessionUp.WaitOne();
            _logger.LogDebug("Solace send {who}->{dto}->{topic}", who, dto, topic);
            var returnCode = Pub(dto, topic, transId);
            return GenPubResponse(returnCode, topic, who, dto);
        }

        public ReqResponse<MessageReplyResult<TResponse>> Req<TRequest, TResponse>(TRequest dto, Topic topic, TimeSpan timeOut, string who, string transId)
        {
            _sessionUp.WaitOne();
            _logger.LogDebug("Solace send {who}->{dto}->{topic}for reply", who, dto, topic);
            return Req<TRequest, TResponse>(dto, topic, timeOut, transId);
        }

        private PubResponse GenPubResponse(ReturnCode returnCode, IMessageBox where, string who, object what)
        {
            if (returnCode == ReturnCode.SOLCLIENT_OK)
            {
                Interlocked.Increment(ref _sentOkCount);
                return new PubResponse
                {
                    IsSuccess = true,
                };
            }
            else
            {
                Interlocked.Increment(ref _sentFailCount);
                OnPubFail(returnCode, where, who, what);
                return new PubResponse
                {
                    IsSuccess = false,
                    ErrorMsg = returnCode.ToString()
                };
            }
        }

        private void OnPubFail(ReturnCode returnCode, IMessageBox where, string who, object what)
        {
            var pubFail = this.PubFail;
            pubFail?.Invoke(this, new PubFailArgs
            {
                Who = who,
                Where = where,
                ErrorMsg = returnCode.ToString(),
                What = what
            });
        }
        private ReqResponse<MessageReplyResult<TResponse>> Req<TRequest, TResponse>(TRequest dto, Topic topic, TimeSpan time0ut, string transId)
        {
            using var message = ContextFactory.Instance.CreateMessage();
            message.Destination = ContextFactory.Instance.CreateTopic(topic.Name);
            message.BinaryAttachment = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));
            message.CreateUserPropertyMap();
            message.UserPropertyMap.AddString(Consts.TransId, transId);

            var returnCode = _session.SendRequest(message, out IMessage response, (int)time0ut.TotalMilliseconds);
            if (returnCode == ReturnCode.SOLCLIENT_OK)
            {
                using (response)
                {
                    var result = new ReqResponse<MessageReplyResult<TResponse>>
                    {
                        Result = Extract(response).GetBodyObj<MessageReplyResult<TResponse>>(),
                        IsSuccess = true,
                    };
                }
            }
            Interlocked.Increment(ref _sentFailCount);
            return new ReqResponse<MessageReplyResult<TResponse>>
            {
                IsSuccess = false,
                ErrorMsg = returnCode.ToString(),
            };
        }
    }
}
