using System.Collections.Generic;
using System.Linq;

namespace Lumin.MQ.Solace
{
    public partial class SolaceMqHub
    {
        #region Collection
        public IReadOnlyCollection<Queue> Queues => _queueListeners.Values.Select(x => x.MessageBox as Queue).ToList();

        public IReadOnlyCollection<Topic> Topics => _topicSubscribers.Values.Select(x => x.MessageBox as Topic).ToList();

        public IReadOnlyCollection<Topic> TopicsNeedReply => _topicRepliers.Values.Select(x => x.Topic).ToList();
        #endregion
    }
}