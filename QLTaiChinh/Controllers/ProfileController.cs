using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTaiChinh.Data;
using QLTaiChinh.Helper;

namespace QLTaiChinh.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly QuanLyTaiChinhCaNhanContext _db;
        public ProfileController(QuanLyTaiChinhCaNhanContext context):base(context) => _db = context;

        private int? UserId() => HttpContext.Session.GetInt32("UserID");

        public async Task<IActionResult> Profile()
        {
            int? uid = UserId();
            if (uid == null) return RedirectToAction("Login", "Login");

            var user = await _db.NguoiDungs.FindAsync(uid.Value);
            if (user == null) return RedirectToAction("Login", "Login");

            // Thống kê nhanh
            var gds = await _db.GiaoDiches.Where(g => g.NguoiDungId == uid).ToListAsync();
            ViewBag.TongGiaoDich = gds.Count;
            ViewBag.TongThu = gds.Where(g => g.LoaiGiaoDich == "Thu").Sum(g => g.SoTien);
            ViewBag.TongChi = gds.Where(g => g.LoaiGiaoDich == "Chi").Sum(g => g.SoTien);
            ViewBag.SoTaiKhoan = await _db.TaiKhoans.CountAsync(t => t.NguoiDungId == uid && t.TrangThai);

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInfo(string HoTen, string? NgaySinh, string? GioiTinh)
        {
            int? uid = UserId();
            if (uid == null) return RedirectToAction("Login", "Login");

            var user = await _db.NguoiDungs.FindAsync(uid.Value);
            if (user == null) return NotFound();

            user.HoTen = HoTen.Trim();
            user.GioiTinh = GioiTinh;
            user.NgayCapNhat = DateTime.Now;
            if (!string.IsNullOrEmpty(NgaySinh) && DateOnly.TryParse(NgaySinh, out var ns))
                user.NgaySinh = ns;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật thông tin!";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string MatKhauCu, string MatKhauMoi, string XacNhan)
        {
            int? uid = UserId();
            if (uid == null) return RedirectToAction("Login", "Login");

            var user = await _db.NguoiDungs.FindAsync(uid.Value);
            if (user == null) return NotFound();

            if (user.MatKhauHash != HashHelper.GetMD5(MatKhauCu.Trim()))
            {
                TempData["Error"] = "Mật khẩu cũ không đúng!";
                return RedirectToAction(nameof(Profile));
            }

            if (MatKhauMoi.Trim() != XacNhan.Trim())
            {
                TempData["Error"] = "Mật khẩu mới và xác nhận không khớp!";
                return RedirectToAction(nameof(Profile));
            }

            if (MatKhauMoi.Trim().Length < 6)
            {
                TempData["Error"] = "Mật khẩu mới phải có ít nhất 6 ký tự!";
                return RedirectToAction(nameof(Profile));
            }

            user.MatKhauHash = HashHelper.GetMD5(MatKhauMoi.Trim());
            user.NgayCapNhat = DateTime.Now;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(Profile));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }
    }
}
