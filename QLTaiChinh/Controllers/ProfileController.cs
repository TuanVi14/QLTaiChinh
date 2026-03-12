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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatAnh(ProfileViewModel vm)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Login");

            var user = _db.NguoiDungs.Find(userId);
            if (user == null) return NotFound();

            if (vm.AnhDaiDienFile != null && vm.AnhDaiDienFile.Length > 0)
            {
                // Kiểm tra định dạng
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var ext = Path.GetExtension(vm.AnhDaiDienFile.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    TempData["Error"] = "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif)";
                    return RedirectToAction("Profile");
                }

                // Kiểm tra kích thước (tối đa 2MB)
                if (vm.AnhDaiDienFile.Length > 2 * 1024 * 1024)
                {
                    TempData["Error"] = "Ảnh không được vượt quá 2MB";
                    return RedirectToAction("Profile");
                }

                // Xóa ảnh cũ nếu không phải ảnh mặc định
                if (!string.IsNullOrEmpty(user.AnhDaiDien) && !user.AnhDaiDien.Contains("face15.jpg"))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, user.AnhDaiDien.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // Lưu ảnh mới
                var fileName = $"avatar_{userId}_{DateTime.Now.Ticks}{ext}";
                var uploadFolder = Path.Combine(_env.WebRootPath, "images", "avatars");
                Directory.CreateDirectory(uploadFolder); // tạo thư mục nếu chưa có
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await vm.AnhDaiDienFile.CopyToAsync(stream);

                user.AnhDaiDien = $"/images/avatars/{fileName}";
            }

            user.NgayCapNhat = DateTime.Now;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Cập nhật ảnh đại diện thành công!";
            return RedirectToAction("Profile");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }
    }
}
