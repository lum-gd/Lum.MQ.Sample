using System;
using System.Collections.Generic;

namespace Lum.MQ.Solace.AspNetCore
{
    public interface IMqHubProvider : IDisposable
    {
        IReadOnlyDictionary<string, IMqHub> Hubs { get; }
    }
}