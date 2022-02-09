using Lum.MQ.Solace;
using Lum.MQ.Solace.AspNetCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lum.MQ.Sample.HubIniters
{
    public class ShenZhenHubIniter : IHubIniter
    {
        public ShenZhenHubIniter(ILogger<ShenZhenHubIniter> logger)
        {
            _logger = logger;
        }
        private readonly ILogger _logger;
        public static List<string> qmsgs => _qmsgs.ToList();
        public static List<int> tmsgs => _tmsgs.ToList();
        public static List<int> rmsgs => _rmsgs.ToList();
        public static List<int> mmsgs => _mmsgs.ToList();

        static ConcurrentQueue<string> _qmsgs = new ConcurrentQueue<string>();
        static BlockingCollection<int> _tmsgs = new BlockingCollection<int>();
        static BlockingCollection<int> _rmsgs = new BlockingCollection<int>();
        static BlockingCollection<int> _mmsgs = new BlockingCollection<int>();

        public string HubName { get { return "shenzhen"; } }

        public void SubQueue(IMqHub mqHub)
        {
            var executionDataflowBlockOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 10,
                MaxDegreeOfParallelism = 10, //Environment.ProcessorCount,
                                             //MaxMessagesPerTask = 1
            };
            mqHub.Sub<int>(HandleQueue, MyQueues.q0, "q0", executionDataflowBlockOptions);
            mqHub.Sub<int>(HandleQueueAsync, MyQueues.q0, "q0Async", executionDataflowBlockOptions);

            _logger.LogInformation("{hubName} SubQueue", HubName);
        }
        public void SubTopic(IMqHub mqHub)
        {
            var executionDataflowBlockOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 10,
                MaxDegreeOfParallelism = 10, //Environment.ProcessorCount,
                                             //MaxMessagesPerTask = 1
            };
            mqHub.Sub<int>(HandleTopic, MyTopics.topic1, "topic1", executionDataflowBlockOptions);
            //mqHub.SubAndReply<int,string>(ReplyTopic,MyTopics.rrtopic);            
            mqHub.Sub<int>(HandleMappedTopic, MyTopics.mappedtopic, "mappedtopic", executionDataflowBlockOptions);

            mqHub.Sub<int>(HandleTopicAsync, MyTopics.topic1, "topic1Async", executionDataflowBlockOptions);
            mqHub.SubAndReply<int, string>(ReplyTopicAsync, MyTopics.rrtopic, "rrtopicAsync", executionDataflowBlockOptions);
            mqHub.Sub<int>(HandleMappedTopicAsync, MyTopics.mappedtopic, "mappedtopicAsync", executionDataflowBlockOptions);

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

        void HandleTopic(int x)
        {
            _tmsgs.Add(x);
            //Thread.Sleep(100);
        }

        async Task HandleTopicAsync(int x)
        {
            _tmsgs.Add(x);
            //await Task.Delay(100);
        }

        string ReplyTopic(int x)
        {
            _rmsgs.Add(x);
            //Thread.Sleep(100);
            return $"received {x} at {DateTime.Now}";
        }

        async Task<string> ReplyTopicAsync(int x)
        {
            rmsgs.Add(x);
            //await Task.Delay(5000);
            return await Task.FromResult($"received {x} at {DateTime.Now}");
        }

        void HandleMappedTopic(int x)
        {
            _mmsgs.Add(x);
            //Thread.Sleep(180);
        }

        async Task HandleMappedTopicAsync(int x)
        {
            _mmsgs.Add(x);
            //await Task.Delay(10O);
        }
    }
}
