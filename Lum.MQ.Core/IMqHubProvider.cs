using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lum.MQ.Core
{
    public interface IMqHubProvider : IDisposable
    {
        IReadOnlyDictionary<string, IMqHub> Hubs { get; }
    }
}
