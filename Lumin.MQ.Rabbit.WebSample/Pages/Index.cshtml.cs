using Lumin.MQ.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Lumin.MQ.Rabbit.WebSample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IMqHubProvider mqHubProvider)
        {
            _logger = logger;
            _hub = mqHubProvider.Hubs[MyHubs.ShangHai];
        }

        public void OnGet()
        {
            var kk = _hub as IQueueCollection;
            Info = _hub.HubName + Environment.NewLine +
                "Queues:" + JsonSerializer.Serialize(kk.Queues, IndentedOptions) + Environment.NewLine +
                "Topics:" + JsonSerializer.Serialize((_hub as ITopicCollection).Topics, IndentedOptions) + Environment.NewLine +
                "TopicsNeedReply;" + JsonSerializer.Serialize((_hub as ITopicCollection).TopicsNeedReply, IndentedOptions) + Environment.NewLine +
                "Received From Oueue:" + JsonSerializer.Serialize(ShangHaiHubIniter.qmsgs, IndentedOptions) + Environment.NewLine;
        }
        IMqHub _hub;
        public string Info { get; set; }
        JsonSerializerOptions IndentedOptions = new JsonSerializerOptions { WriteIndented = true };
    }
}