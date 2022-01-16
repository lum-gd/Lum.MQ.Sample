using System;

namespace Lumin.MQ.Solace
{
    public interface IMessageHandlerBase
    {
        void Complete();
        long SuccessCount { get; }
        long ErrorCount { get; }
    }
}
