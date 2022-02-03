using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Lumin.MQ.Core;

namespace Lumin.MQ.Rabbit.WorkerSample
{
    public class ShangHaiHubIniter : IHubIniter
    {
        public ShangHaiHubIniter(ILogger<ShangHaiHubIniter> logger)
        {
            _logger = logger;
        }
        private readonly ILogger _logger;
        public static List<string> qmsgs => _qmsgs.ToList();

        static ConcurrentQueue<string> _qmsgs = new ConcurrentQueue<string>();

        public string HubName { get { return MyHubs.ShangHai; } }

        public void SubQueue(IMqHub mqHub)
        {
            var executionDataflowBlockOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 10,
                MaxDegreeOfParallelism = 10, //Environment.ProcessorCount,
                                             //MaxMessagesPerTask = 1
            };
            mqHub.Sub<int>(HandleQueue, MyQueues.hello, "q0", executionDataflowBlockOptions);
            //mqHub.Sub<int>(HandleQueueAsync, MyQueues.hello, "q0Async", executionDataflowBlockOptions);

            _logger.LogInformation("{hubName} SubQueue", HubName);
        }
        public void SubTopic(IMqHub mqHub)
        {
            _logger.LogInformation("{hubName} SubTopic", HubName);
        }

        void HandleQueue(int x)
        {
            _qmsgs.Enqueue(x.ToString());
            //throw new Exception("mock exception");
        }

        async Task HandleQueueAsync(int x)
        {
            _qmsgs.Enqueue(x + "Async");
            //await Task.Delay(10);
            await Task.CompletedTask;
        }
    }
}
