using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainTracking.Infrastructure.Persistence;

namespace TrainTracking.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Health check endpoint for Railway
        [Route("/health")]
        public IActionResult Health()
        {
            return Ok("Healthy");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
