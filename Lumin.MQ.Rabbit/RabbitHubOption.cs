using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumin.MQ.Rabbit
{
    public class RabbitHubOption
    {
        public string HubName { get; set; }
        public string Host { get; set; }
        public List<ConcurrencyOption> ConcurrencyOptions { get; set; } = new List<ConcurrencyOption>();
        public RabbitQueueOption[] RabbitQueueOptions { get; set; }
    }

    public class ConcurrencyOption
    {
        public string Name { get; set; }
        public int BoundedCapacity { get; set; }
        //public int MaxDegree0fParallelism { get; set;}
        //public int MaxMessagesPerTask{ get; set;}
    }

    public class RabbitQueueOption
    {
        public string Name { get; set; }
        public bool WillSubscribeQueue { get; set; }
    }
}
