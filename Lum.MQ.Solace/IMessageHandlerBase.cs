using System;

namespace Lum.MQ.Solace
{
    public interface IMessageHandlerBase
    {
        void Complete();
        long SuccessCount { get; }
        long ErrorCount { get; }
    }
}
