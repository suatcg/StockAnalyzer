using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using StockAnalyzer.Core;

namespace StockAnalyzer.Web.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "Home Page";

            // Let's make sure that we can load the files when you start the project!
            var store = new DataStore(HostingEnvironment.MapPath("~/bin"));

            await store.LoadStocks();

            return View();
        }

        // Big benefit using async and await inside ASP.NET is to relieve ISS or the web server that you're using so that it can go ahead and work with other requests as your as your data is being unloaded from disk, the database or from another API.
        [Route("Stock/{ticker}")]
        public async Task<ActionResult> Stock(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker)) ticker = "MSFT";

            ViewBag.Title = $"Stock Details for {ticker}";

            var store = new DataStore(HostingEnvironment.MapPath("~/bin"));

            var data = await store.LoadStocks();

            return View(data[ticker]);
        }
    }
}
