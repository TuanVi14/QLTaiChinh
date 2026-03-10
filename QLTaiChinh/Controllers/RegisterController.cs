using Microsoft.AspNetCore.Mvc;
using QLTaiChinh.Data;

namespace QLTaiChinh.Controllers
{
    public class RegisterController : Controller
    {
        private readonly QuanLyTaiChinhCaNhanContext db;
        public RegisterController(QuanLyTaiChinhCaNhanContext context)
        {
            this.db = context;
        }
        public IActionResult Register()
        {
            return View();
        }
    }
}
