using Microsoft.AspNetCore.Mvc;

namespace CodeLearn.Controllers
{
    public class CourcesController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Courses");
        }
    }
}
