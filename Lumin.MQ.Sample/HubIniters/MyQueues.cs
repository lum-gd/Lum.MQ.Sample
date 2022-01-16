using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lumin.MQ.Sample.HubIniters
{
    public static class Consts
    {
        public static string SolaceHubOptions = nameof(SolaceHubOptions);
    }

    public static class MyHubs
    {
        public static string BeiJing = nameof(BeiJing);
        public static string Shenzhen = nameof(Shenzhen);
    }

    public static class MyQueues
    {
        public static Queue q0 = new Queue { Name = "pushas/req/template/que/v0" };
        public static Queue q1 = new Queue { Name = "pushas/req/template/que/v1" };
        public static Queue q2 = new Queue { Name = "pushas/req/template/que/v2" };
    }

    public static class MyTopics
    {
        public static Topic topic1 = new Topic { Name = "sample/topicname" };
        public static Topic rrtopic = new Topic { Name = "sample/rrtopic" };
        public static Topic mappedtopic = new Topic { Name = "q1/mappedtopic" };
    }
}
