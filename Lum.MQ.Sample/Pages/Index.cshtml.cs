using Lum.MQ.Sample.HubIniters;
using Lum.MQ.Solace.AspNetCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace Lum.MQ.Sample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IMqHubProvider mqHubProvider)
        {
            _logger = logger;
            _hub = mqHubProvider.Hubs[MyHubs.Shenzhen];
        }
        public void OnGet()
        {
            Info = _hub.HubName + Environment.NewLine +
                "Queues:" + JsonSerializer.Serialize((_hub as IQueueCollection).Queues, IndentedOptions) + Environment.NewLine +
                "Topics:" + JsonSerializer.Serialize((_hub as ITopicCollection).Topics, IndentedOptions) + Environment.NewLine +
                "TopicsNeedReply;" + JsonSerializer.Serialize((_hub as ITopicCollection).TopicsNeedReply, IndentedOptions) + Environment.NewLine +
                "Received Erom Oueue:" + JsonSerializer.Serialize(ShenZhenHubIniter.qmsgs, IndentedOptions) + Environment.NewLine +
                "Received From Topic;" + JsonSerializer.Serialize(ShenZhenHubIniter.tmsgs, IndentedOptions) + Environment.NewLine +
                "Received From NeedReply:" + JsonSerializer.Serialize(ShenZhenHubIniter.rmsgs, IndentedOptions) + Environment.NewLine +
                "Received From MappedTopiC:" + JsonSerializer.Serialize(ShenZhenHubIniter.mmsgs, IndentedOptions) + Environment.NewLine;

        }
        IMqHub _hub;//IQueueCollection ITopicCollection
        public string Info { get; set; }
        JsonSerializerOptions IndentedOptions = new JsonSerializerOptions { WriteIndented = true };
    }
}