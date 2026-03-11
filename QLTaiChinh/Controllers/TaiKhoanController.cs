using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTaiChinh.Data;

namespace QLTaiChinh.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly QuanLyTaiChinhCaNhanContext _db;
        public TaiKhoanController(QuanLyTaiChinhCaNhanContext context) => _db = context;

        private int? UserId() => HttpContext.Session.GetInt32("UserID");
        public async Task<IActionResult> TaiKhoan()
        {
            int? uid = UserId();
            if (uid == null) return RedirectToAction("Login", "Login");

            var list = await _db.TaiKhoans
                .Where(t => t.NguoiDungId == uid)
                .OrderByDescending(t => t.TrangThai).ThenBy(t => t.TenTaiKhoan)
                .ToListAsync();

            ViewBag.TongSoDu = list.Where(t => t.TrangThai).Sum(t => t.SoDu);
            ViewBag.SoTaiKhoan = list.Count(t => t.TrangThai);
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string TenTaiKhoan, string LoaiTaiKhoan, decimal SoDu)
        {
            int? uid = UserId();
            if (uid == null) return RedirectToAction("Login", "Login");

            _db.TaiKhoans.Add(new TaiKhoan
            {
                NguoiDungId = uid.Value,
                TenTaiKhoan = TenTaiKhoan.Trim(),
                LoaiTaiKhoan = LoaiTaiKhoan,
                SoDu = SoDu,
                TrangThai = true,
                NgayTao = DateTime.Now,
                NgayCapNhat = DateTime.Now
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã thêm tài khoản \"{TenTaiKhoan}\"!";
            return RedirectToAction(nameof(TaiKhoan));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string TenTaiKhoan, string LoaiTaiKhoan, decimal SoDu)
        {
            int? uid = UserId();
            if (uid == null) return RedirectToAction("Login", "Login");

            var tk = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.TaiKhoanId == id && t.NguoiDungId == uid);
            if (tk != null)
            {
                tk.TenTaiKhoan = TenTaiKhoan.Trim();
                tk.LoaiTaiKhoan = LoaiTaiKhoan;
                tk.SoDu = SoDu;
                tk.NgayCapNhat = DateTime.Now;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật tài khoản!";
            }
            return RedirectToAction(nameof(TaiKhoan));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            int? uid = UserId();
            if (uid == null) return RedirectToAction("Login", "Login");

            var tk = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.TaiKhoanId == id && t.NguoiDungId == uid);
            if (tk != null)
            {
                tk.TrangThai = !tk.TrangThai;
                tk.NgayCapNhat = DateTime.Now;
                await _db.SaveChangesAsync();
                TempData["Success"] = tk.TrangThai ? "Đã kích hoạt tài khoản!" : "Đã vô hiệu hoá tài khoản!";
            }
            return RedirectToAction(nameof(TaiKhoan));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int? uid = UserId();
            if (uid == null) return RedirectToAction("Login", "Login");

            var tk = await _db.TaiKhoans
                .Include(t => t.GiaoDiches)
                .FirstOrDefaultAsync(t => t.TaiKhoanId == id && t.NguoiDungId == uid);

            if (tk != null)
            {
                if (tk.GiaoDiches.Any())
                {
                    TempData["Error"] = "Không thể xoá tài khoản đã có giao dịch. Hãy vô hiệu hoá thay thế.";
                }
                else
                {
                    _db.TaiKhoans.Remove(tk);
                    await _db.SaveChangesAsync();
                    TempData["Success"] = "Đã xoá tài khoản!";
                }
            }
            return RedirectToAction(nameof(TaiKhoan));
        }
    }
}
