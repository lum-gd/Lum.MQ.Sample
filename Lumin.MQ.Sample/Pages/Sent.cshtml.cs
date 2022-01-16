using Lumin.MQ.Sample.HubIniters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lumin.MQ.Sample.Pages
{
    public class SentModel : PageModel
    {
        private readonly ILogger _logger;

        public SentModel(ILogger<SentModel> logger, IMqHub mqHub)
        {
            _logger = logger;
            _hub = mqHub;
        }

        public void OnGet()
        {
            int width = 1;
            int len = 1;

            Stopwatch sw = Stopwatch.StartNew();
            Parallel.For(0, width, i =>
            {
                for (int j = 0; j < len; j++)
                {
                    var sentItem = new SentItem
                    {
                        Index = i * 100 + j,
                        SentTime = DateTime.Now,
                    };
                    Stopwatch sw = Stopwatch.StartNew();
                    sentItem.Response = _hub.Req<int, string>(sentItem.Index, MyTopics.rrtopic, TimeSpan.FromSeconds(1), "jerry", "transId_" + sentItem.Index.ToString());
                    sw.Stop();
                    sentItem.ms = sw.ElapsedMilliseconds;
                    sentItems.Enqueue(sentItem);
                }
            });
            sw.Stop();

            var list = sentItems.OrderByDescending(x => x.SentTime).ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.AppendLine(item.Index + " - " + item.ms + " - " + item.Response.IsSuccess);
            }

            Info = _hub.HubName + Environment.NewLine +
                "Sent: " + JsonSerializer.Serialize(msgs, IndentedOptions) + Environment.NewLine +
                "Reply: " + sentItems.Count + " ones " + Environment.NewLine +
                " cost time " + sw.ElapsedMilliseconds + Environment.NewLine +
                list.First().Index + " - " + list.First().SentTime + " - " + list.First().Response.Result.Result + Environment.NewLine +
                list.Last().Index + " - " + list.Last().SentTime + " - " + list.Last().Response.Result.Result + Environment.NewLine +
                sb.ToString() + Environment.NewLine + Environment.NewLine;
        }

        ConcurrentQueue<SentItem> sentItems = new ConcurrentQueue<SentItem>();
        JsonSerializerOptions IndentedOptions = new JsonSerializerOptions { WriteIndented = true };
        static List<int> msgs = new List<int>();

        IMqHub _hub;

        static int index = 0;

        public string Info { get; set; }

        public class SentItem
        {
            public int Index { get; set; }
            public DateTime SentTime { get; set; }
            public ReqResponse<MessageReplyResult<string>> Response { get; set; }
            public long ms { get; set; }
        }
    }
}
