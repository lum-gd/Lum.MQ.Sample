using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lumin.MQ.Rabbit.WebSample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
