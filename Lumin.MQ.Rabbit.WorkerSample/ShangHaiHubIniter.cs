using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Lum.MQ.Core;

namespace Lum.MQ.Rabbit.WorkerSample
{
    public class ShangHaiHubIniter : IHubIniter
    {
        public ShangHaiHubIniter(ILogger<ShangHaiHubIniter> logger)
        {
            _logger = logger;
        }
        private readonly ILogger _logger;

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
            _logger.LogDebug("received {x}", x);
            //throw new Exception("mock exception");
        }

        async Task HandleQueueAsync(int x)
        {
            _logger.LogDebug("async received {x}", x);
            //await Task.Delay(10);
            await Task.CompletedTask;
        }
    }
}
