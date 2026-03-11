using Microsoft.AspNetCore.Mvc;
using QLTaiChinh.Data;
using QLTaiChinh.Helper;
using QLTaiChinh.Models;

namespace QLTaiChinh.Controllers
{
    public class RegisterController : Controller
    {
        private readonly QuanLyTaiChinhCaNhanContext db;
        public RegisterController(QuanLyTaiChinhCaNhanContext context)
        {
            db = context;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(DangKyViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.NguoiDungs.Any(n => n.Email == model.Email.Trim()))
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký!");
                    return View(model);
                }
                var nd = new NguoiDung
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    MatKhauHash = HashHelper.GetMD5(model.Password.Trim()),
                    TrangThai = true,
                    NgayTao = DateTime.Now,
                    NgayCapNhat = DateTime.Now
                };
                db.NguoiDungs.Add(nd);
                db.SaveChanges();
                return RedirectToAction("Login", "Login");
            }
            return View(model);
        }
    }
}
