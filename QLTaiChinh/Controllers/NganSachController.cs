using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLTaiChinh.Data;

namespace QLTaiChinh.Controllers
{
    public class NganSachController : Controller
    {
        private readonly QuanLyTaiChinhCaNhanContext _db;

        public NganSachController(QuanLyTaiChinhCaNhanContext context)
        {
            _db = context;
        }

        private int? GetUserId() => HttpContext.Session.GetInt32("UserID");

        // ── Danh sách ngân sách ──────────────────────────────────────
        public async Task<IActionResult> NganSach(int? thang, int? nam)
        {
            int? userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Login");

            int t = thang ?? DateTime.Now.Month;
            int y = nam ?? DateTime.Now.Year;

            var list = await _db.NganSaches
                .Include(n => n.DanhMuc)
                .Where(n => n.NguoiDungId == userId && n.Thang == t && n.Nam == y)
                .OrderByDescending(n => n.SoTienDaChiTieu * 100m / n.SoTienGioiHan)
                .ToListAsync();

            ViewBag.Thang = t;
            ViewBag.Nam = y;
            ViewBag.TongGioiHan = list.Sum(n => n.SoTienGioiHan);
            ViewBag.TongDaChiTieu = list.Sum(n => n.SoTienDaChiTieu);

            // Danh sách năm
            var allNgay = await _db.GiaoDiches
                .Where(g => g.NguoiDungId == userId)
                .Select(g => g.NgayGiaoDich).ToListAsync();
            ViewBag.NamList = allNgay.Select(d => d.Year).Distinct().OrderByDescending(x => x).ToList();

            return View(list);
        }

        // ── Thêm ngân sách ───────────────────────────────────────────
        public async Task<IActionResult> Create(int? thang, int? nam)
        {
            int? userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Login");
            await LoadDanhMucDropdown(userId.Value);
            ViewBag.DefaultThang = thang ?? DateTime.Now.Month;
            ViewBag.DefaultNam = nam ?? DateTime.Now.Year;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NganSach model)
        {
            int? userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Login");

            // Kiểm tra trùng
            bool exists = await _db.NganSaches.AnyAsync(n =>
                n.NguoiDungId == userId &&
                n.DanhMucId == model.DanhMucId &&
                n.Thang == model.Thang &&
                n.Nam == model.Nam);

            if (exists)
                ModelState.AddModelError("", "Danh mục này đã có ngân sách trong tháng đã chọn.");

            if (ModelState.IsValid)
            {
                model.NguoiDungId = userId.Value;
                model.NgayTao = DateTime.Now;
                model.NgayCapNhat = DateTime.Now;
                _db.NganSaches.Add(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Thêm ngân sách thành công!";
                return RedirectToAction(nameof(NganSach), new { thang = model.Thang, nam = model.Nam });
            }

            await LoadDanhMucDropdown(userId.Value, model.DanhMucId);
            return View(model);
        }

        // ── Chỉnh sửa ────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            int? userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Login");

            var ns = await _db.NganSaches
                .FirstOrDefaultAsync(n => n.NganSachId == id && n.NguoiDungId == userId);
            if (ns == null) return NotFound();

            await LoadDanhMucDropdown(userId.Value, ns.DanhMucId);
            return View(ns);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NganSach model)
        {
            int? userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Login");

            if (ModelState.IsValid)
            {
                var existing = await _db.NganSaches
                    .FirstOrDefaultAsync(n => n.NganSachId == id && n.NguoiDungId == userId);
                if (existing == null) return NotFound();

                existing.SoTienGioiHan = model.SoTienGioiHan;
                existing.NgayCapNhat = DateTime.Now;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Cập nhật ngân sách thành công!";
                return RedirectToAction(nameof(NganSach), new { thang = existing.Thang, nam = existing.Nam });
            }

            await LoadDanhMucDropdown(userId.Value, model.DanhMucId);
            return View(model);
        }

        // ── Xoá ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int? userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Login");

            var ns = await _db.NganSaches
                .FirstOrDefaultAsync(n => n.NganSachId == id && n.NguoiDungId == userId);
            if (ns != null)
            {
                _db.NganSaches.Remove(ns);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Đã xoá ngân sách!";
            }
            return RedirectToAction(nameof(NganSach));
        }

        private async Task LoadDanhMucDropdown(int userId, int? selected = null)
        {
            var danhMucs = await _db.DanhMucs
                .Where(d => d.TrangThai && d.LoaiDanhMuc == "Chi")
                .OrderBy(d => d.TenDanhMuc)
                .ToListAsync();
            ViewBag.DanhMucList = new SelectList(danhMucs, "DanhMucId", "TenDanhMuc", selected);
        }
    }
}
