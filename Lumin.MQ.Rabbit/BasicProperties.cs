using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumin.MQ.Rabbit
{
    public struct BasicProperties : IBasicProperties
    {
        public string? ContentType { get; set; }
        public string? ContentEncoding { get; set; }
        public IDictionary<string, object?>? Headers { get; set; }
        public byte DeliveryMode { get; set; }
        public byte Priority { get; set; }
        public string? CorrelationId { get; set; }
        public string? ReplyTo { get; set; }
        public string? Expiration { get; set; }
        public string? MessageId { get; set; }
        public AmqpTimestamp Timestamp { get; set; }
        public string? Type { get; set; }
        public string? UserId { get; set; }
        public string? AppId { get; set; }
        public string? ClusterId { get; set; }

        public bool Persistent
        {
            readonly get { return DeliveryMode == 2; }
            set { DeliveryMode = value ? (byte)2 : (byte)1; }
        }

        public PublicationAddress? ReplyToAddress
        {
            readonly get
            {
                PublicationAddress.TryParse(ReplyTo, out PublicationAddress result);
                return result;
            }
            set { ReplyTo = value?.ToString(); }
        }

        public ushort ProtocolClassId => 0;

        public string ProtocolClassName => string.Empty;

        public void ClearContentType() => ContentType = default;
        public void ClearContentEncoding() => ContentEncoding = default;
        public void ClearHeaders() => Headers = default;
        public void ClearDeliveryMode() => DeliveryMode = default;
        public void ClearPriority() => Priority = default;
        public void ClearCorrelationId() => CorrelationId = default;
        public void ClearReplyTo() => ReplyTo = default;
        public void ClearExpiration() => Expiration = default;
        public void ClearMessageId() => MessageId = default;
        public void ClearTimestamp() => Timestamp = default;
        public void ClearType() => Type = default;
        public void ClearUserId() => UserId = default;
        public void ClearAppId() => AppId = default;
        public void ClearClusterId() => ClusterId = default;

        public readonly bool IsContentTypePresent() => ContentType != default;
        public readonly bool IsContentEncodingPresent() => ContentEncoding != default;
        public readonly bool IsHeadersPresent() => Headers != default;
        public readonly bool IsDeliveryModePresent() => DeliveryMode != default;
        public readonly bool IsPriorityPresent() => Priority != default;
        public readonly bool IsCorrelationIdPresent() => CorrelationId != default;
        public readonly bool IsReplyToPresent() => ReplyTo != default;
        public readonly bool IsExpirationPresent() => Expiration != default;
        public readonly bool IsMessageIdPresent() => MessageId != default;
        public readonly bool IsTimestampPresent() => this.Timestamp.UnixTime != 0;
        public readonly bool IsTypePresent() => Type != default;
        public readonly bool IsUserIdPresent() => UserId != default;
        public readonly bool IsAppIdPresent() => AppId != default;
        public readonly bool IsClusterIdPresent() => ClusterId != default;

        //----------------------------------
        // First byte
        //----------------------------------
        internal const byte ContentTypeBit = 7;
        internal const byte ContentEncodingBit = 6;
        internal const byte HeaderBit = 5;
        internal const byte DeliveryModeBit = 4;
        internal const byte PriorityBit = 3;
        internal const byte CorrelationIdBit = 2;
        internal const byte ReplyToBit = 1;
        internal const byte ExpirationBit = 0;

        //----------------------------------
        // Second byte
        //----------------------------------
        internal const byte MessageIdBit = 7;
        internal const byte TimestampBit = 6;
        internal const byte TypeBit = 5;
        internal const byte UserIdBit = 4;
        internal const byte AppIdBit = 3;
        internal const byte ClusterIdBit = 2;
    }
}
