using SolaceSystems.Solclient.Messaging;
using System.Collections.Generic;

namespace Lumin.MQ.Solace
{
    public class SolaceHubOptions
    {
        public SolaceHubOption[] Options { get; set; }
    }

    public class SolaceHubOption
    {
        public string HubName { get; set; }
        public SessionProperties SessionProperties { get; set; }
        public SolaceQueueOption[] SolaceQueueOptions { get; set; }
        public List<string> Topics { get; set; }
        public List<ConcurrencyOption> ConcurrencyOptions { get; set; } = new List<ConcurrencyOption>();
    }

    public class ConcurrencyOption
    {
        public string Name { get; set; }
        public int BoundedCapacity { get; set; }
        //public int MaxDegree0fParallelism { get; set;}
        //public int MaxMessagesPerTask{ get; set;}
    }

    public class SolaceQueueOption
    {
        public string Name { get; set; }
        public bool WillSubscribeQueue { get; set; }
        public List<MappedTopicOption> MappedTopicOptions { get; set; }
        public EndpointProperties EndpointProperties { get; set; }
        public FlowProperties FlowProperties { get; set; }

        public static EndpointProperties DefaultEndpointProperties { get; } = new EndpointProperties
        {
            Permission = EndpointProperties.EndpointPermission.Consume,
            AccessType = EndpointProperties.EndpointAccessType.Exclusive,
        };
        public static FlowProperties DefaultFlowProperties { get; } = new FlowProperties
        {
            AckMode = MessageAckMode.ClientAck
        };
    }

    public class MappedTopicOption
    {
        public bool WillSubscribeTopic { get; set; }
        public Topic Topic { get; set; }
    }
}
