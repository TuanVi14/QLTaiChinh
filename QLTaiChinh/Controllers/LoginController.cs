using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using QLTaiChinh.Data;
using QLTaiChinh.Helper;
using QLTaiChinh.Models;

namespace QLTaiChinh.Controllers
{
    public class LoginController : Controller
    {
        private readonly QuanLyTaiChinhCaNhanContext db;

        public LoginController(QuanLyTaiChinhCaNhanContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel data)
        {
            string passwordHash = HashHelper.GetMD5(data.Password.Trim());
            var nguoidung = db.NguoiDungs
                .SingleOrDefault(nd => nd.Email == data.Email.Trim());

            if (nguoidung != null && nguoidung.MatKhauHash == passwordHash)
            {
                HttpContext.Session.SetInt32("UserID", nguoidung.NguoiDungId);
                return RedirectToAction("TongQuan", "TongQuan");
            }

            ModelState.AddModelError("", "Sai thông tin đăng nhập");
            return View(data);
        }
    }
}