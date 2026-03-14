using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLTaiChinh.Data;

namespace QLTaiChinh.Controllers
{
    public class GiaoDichController : BaseController
    {
        private readonly QuanLyTaiChinhCaNhanContext _db;

        public GiaoDichController(QuanLyTaiChinhCaNhanContext context):base(context) 
        {
            _db = context;
        }

        private int? GetUserId() => HttpContext.Session.GetInt32("UserID");

        // ── Danh sách giao dịch ──────────────────────────────────────
        public async Task<IActionResult> GiaoDich(int? thang, int? nam, string? loai, string? keyword)
        {
            int? userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Login");

            int t = thang ?? DateTime.Now.Month;
            int y = nam ?? DateTime.Now.Year;

            var ngayDau = new DateOnly(y, t, 1);
            var ngayCuoi = ngayDau.AddMonths(1).AddDays(-1);

            var query = _db.GiaoDiches
                .Include(g => g.DanhMuc)
                .Include(g => g.TaiKhoan)
                .Where(g => g.NguoiDungId == userId
                         && g.NgayGiaoDich >= ngayDau
                         && g.NgayGiaoDich <= ngayCuoi);

            if (!string.IsNullOrEmpty(loai))
                query = query.Where(g => g.LoaiGiaoDich == loai);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(g => g.MoTa!.Contains(keyword) || g.DanhMuc.TenDanhMuc.Contains(keyword));

            var list = await query.OrderByDescending(g => g.NgayGiaoDich).ToListAsync();

            // KPI tháng
            ViewBag.TongThu = list.Where(g => g.LoaiGiaoDich == "Thu").Sum(g => g.SoTien);
            ViewBag.TongChi = list.Where(g => g.LoaiGiaoDich == "Chi").Sum(g => g.SoTien);
            ViewBag.Thang = t;
            ViewBag.Nam = y;
            ViewBag.Loai = loai;
            ViewBag.Keyword = keyword;

            // Danh sách năm để lọc
            var allNgay = await _db.GiaoDiches
                .Where(g => g.NguoiDungId == userId)
                .Select(g => g.NgayGiaoDich).ToListAsync();
            ViewBag.NamList = allNgay.Select(d => d.Year).Distinct().OrderByDescending(x => x).ToList();

            return View(list);
        }

        // ── Form thêm mới ────────────────────────────────────────────
        public async Task<IActionResult> Create()
        {
            int? userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Login");
            await LoadDropdowns(userId.Value);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GiaoDich model)
        {
            int? userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Login");

            model.NguoiDungId = userId.Value;

            // Xóa các navigation property khỏi ModelState
            ModelState.Remove("NguoiDung");
            ModelState.Remove("DanhMuc");
            ModelState.Remove("TaiKhoan");

            if (ModelState.IsValid)
            {  
                model.NgayTao = DateTime.Now;
                model.NgayCapNhat = DateTime.Now;
                _db.GiaoDiches.Add(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Thêm giao dịch thành công!";
                return RedirectToAction(nameof(GiaoDich));
            }

            await LoadDropdowns(userId.Value, model.TaiKhoanId, model.DanhMucId);
            return View(model);
        }

        // ── Form chỉnh sửa ───────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            int? userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Login");

            var gd = await _db.GiaoDiches
                .FirstOrDefaultAsync(g => g.GiaoDichId == id && g.NguoiDungId == userId);
            if (gd == null) return NotFound();

            await LoadDropdowns(userId.Value, gd.TaiKhoanId, gd.DanhMucId);
            return View(gd);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GiaoDich model)
        {
            int? userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Login");

            // Xóa các navigation property khỏi ModelState
            ModelState.Remove("NguoiDung");
            ModelState.Remove("DanhMuc");
            ModelState.Remove("TaiKhoan");

            if (ModelState.IsValid)
            {
                var existing = await _db.GiaoDiches
                    .FirstOrDefaultAsync(g => g.GiaoDichId == id && g.NguoiDungId == userId);
                if (existing == null) return NotFound();

                existing.TaiKhoanId = model.TaiKhoanId;
                existing.DanhMucId = model.DanhMucId;
                existing.LoaiGiaoDich = model.LoaiGiaoDich;
                existing.SoTien = model.SoTien;
                existing.NgayGiaoDich = model.NgayGiaoDich;
                existing.MoTa = model.MoTa;
                existing.HinhThucThanhToan = model.HinhThucThanhToan;
                existing.GhiChu = model.GhiChu;
                existing.NgayCapNhat = DateTime.Now;

                await _db.SaveChangesAsync();
                TempData["Success"] = "Cập nhật giao dịch thành công!";
                return RedirectToAction(nameof(GiaoDich));
            }

            await LoadDropdowns(userId.Value, model.TaiKhoanId, model.DanhMucId);
            return View(model);
        }

        // ── Xoá ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int? userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Login");

            var gd = await _db.GiaoDiches
                .FirstOrDefaultAsync(g => g.GiaoDichId == id && g.NguoiDungId == userId);
            if (gd != null)
            {
                _db.GiaoDiches.Remove(gd);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Đã xoá giao dịch!";
            }
            return RedirectToAction(nameof(GiaoDich));
        }

        // ── Helper ───────────────────────────────────────────────────
        private async Task LoadDropdowns(int userId, int? taiKhoanId = null, int? danhMucId = null)
        {
            var taiKhoans = await _db.TaiKhoans
                .Where(t => t.NguoiDungId == userId && t.TrangThai)
                .ToListAsync();
            ViewBag.TaiKhoanList = new SelectList(taiKhoans, "TaiKhoanId", "TenTaiKhoan", taiKhoanId);

            var danhMucs = await _db.DanhMucs
                .Where(d => d.TrangThai)
                .OrderBy(d => d.LoaiDanhMuc).ThenBy(d => d.TenDanhMuc)
                .ToListAsync();
            ViewBag.DanhMucList = new SelectList(
                danhMucs.Select(d => new {
                    d.DanhMucId,
                    Ten = $"[{d.LoaiDanhMuc}] {d.TenDanhMuc}"
                }),
                "DanhMucId", "Ten", danhMucId);

            ViewBag.HinhThucList = new SelectList(new[]
            {
                "Tiền mặt", "Chuyển khoản", "Thẻ tín dụng", "Thẻ ghi nợ", "Ví điện tử", "Khác"
            });
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggest(string q)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return Json(new { success = false });

            if (string.IsNullOrWhiteSpace(q))
                return Json(new { success = true, data = new object[] { } });

            string keyword = q.Trim().ToLower();

            var results = await _db.GiaoDiches
                .Include(g => g.DanhMuc)
                .Where(g => g.NguoiDungId == userId && (
                    (g.MoTa != null && g.MoTa.ToLower().Contains(keyword)) ||
                    (g.GhiChu != null && g.GhiChu.ToLower().Contains(keyword)) ||
                    g.DanhMuc.TenDanhMuc.ToLower().Contains(keyword)
                ))
                .OrderByDescending(g => g.NgayGiaoDich)
                .Take(7)
                .Select(g => new
                {
                    id = g.GiaoDichId,
                    moTa = g.MoTa ?? g.GhiChu ?? "(Khong co mo ta)",
                    soTien = g.SoTien,
                    loai = g.LoaiGiaoDich,
                    danhMuc = g.DanhMuc.TenDanhMuc,
                    ngay = g.NgayGiaoDich.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Json(new { success = true, data = results });
        }

        // -----------------------------------------------
        // GET: /GiaoDich/KetQuaTimKiem?q=cafe
        // -----------------------------------------------
        [HttpGet]
        public async Task<IActionResult> KetQuaTimKiem(string q)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "Login");

            var query = _db.GiaoDiches
                .Include(g => g.DanhMuc)
                .Include(g => g.TaiKhoan)
                .Where(g => g.NguoiDungId == userId);

            if (!string.IsNullOrWhiteSpace(q))
            {
                string keyword = q.Trim().ToLower();
                query = query.Where(g =>
                    (g.MoTa != null && g.MoTa.ToLower().Contains(keyword)) ||
                    (g.GhiChu != null && g.GhiChu.ToLower().Contains(keyword)) ||
                    g.DanhMuc.TenDanhMuc.ToLower().Contains(keyword)
                );
            }

            var ketQua = await query
                .OrderByDescending(g => g.NgayGiaoDich)
                .ToListAsync();

            ViewData["TuKhoa"] = q;
            ViewData["TongKet"] = ketQua.Count;
            return View(ketQua);
        }
    }
}
