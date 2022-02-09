using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lum.MQ.Core
{
    public interface IHubIniter
    {
        string HubName { get; }
        void SubQueue(IMqHub mqHub);
        void SubTopic(IMqHub mqHub);
    }
}
