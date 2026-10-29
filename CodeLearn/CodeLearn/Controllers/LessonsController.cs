using Microsoft.AspNetCore.Mvc;

namespace CodeLearn.Controllers
{
    public class LessonsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
