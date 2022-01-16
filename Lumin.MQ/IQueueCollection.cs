using System.Collections.Generic;

namespace Lumin.MQ
{
    public interface IQueueCollection
    {
        IReadOnlyCollection<Queue> Queues { get; }
    }

    public interface ITopicCollection
    {
        IReadOnlyCollection<Topic> Topics { get; }
        IReadOnlyCollection<Topic> TopicsNeedReply { get; }
    }
}