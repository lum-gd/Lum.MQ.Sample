using System.Text.Json;
namespace Lum.MQ
{
    public interface IReceivedMessageDto
    {
        string TransId { get; }
        string Body { get; }
        IMessageBox From { get; }
        T GetBodyObj<T>();
    }

    public class ReceivedMessageDto : IReceivedMessageDto
    {
        public string TransId { get; set; }
        public string Body { get; set; }
        public IMessageBox From { get; set; }
        public T GetBodyObj<T>()
        {
            return JsonSerializer.Deserialize<T>(Body);
        }
    }

    public class ReqResponse<TResponse>
    {
        public TResponse Result { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class PubResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class MessageReplyResult<T>
    {
        public T Result { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class PubFailArgs
    {
        public string Who { get; set; }
        public object What { get; set; }
        public IMessageBox Where { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class SubFailArgs
    {
        public string Hho { get; set; }
        public IReceivedMessageDto What { get; set; }
        public IMessageBox Where { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class AckEventArgs
    {
        public object CorrelationKey { get; set; }
        public bool IsAccepted { get; set; }
    }
}