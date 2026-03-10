using Microsoft.AspNetCore.Mvc;

namespace QLTaiChinh.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Profile()
        {
            return View();
        }
    }
}
