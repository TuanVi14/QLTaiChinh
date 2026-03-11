using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTaiChinh.Data;
using QLTaiChinh.Models;

namespace QLTaiChinh.Controllers
{
    public class TongQuanController : Controller
    {
        private readonly QuanLyTaiChinhCaNhanContext _db;

        public TongQuanController(QuanLyTaiChinhCaNhanContext context)
        {
            _db = context;
        }
        public async Task<IActionResult> TongQuan(int? thang, int? nam)
        {

            // ── Xác định người dùng từ Session ───────────────────────
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Login");

            int targetThang = thang ?? DateTime.Now.Month;
            int targetNam = nam ?? DateTime.Now.Year;

            // ── Lấy tất cả giao dịch trong tháng ────────────────────
            // So sánh khoảng DateOnly thay vì .Month/.Year (EF Core không dịch được sang SQL)
            var ngayDau = new DateOnly(targetNam, targetThang, 1);
            var ngayCuoi = ngayDau.AddMonths(1).AddDays(-1);

            var giaoDichs = await _db.GiaoDiches
                .Include(g => g.DanhMuc)
                .ThenInclude(d => d.DanhMucCha)
                .Where(g => g.NguoiDungId == userId
                         && g.NgayGiaoDich >= ngayDau
                         && g.NgayGiaoDich <= ngayCuoi)
                .ToListAsync();

            // ── KPI ──────────────────────────────────────────────────
            decimal tongThu = giaoDichs.Where(g => g.LoaiGiaoDich == "Thu").Sum(g => g.SoTien);
            decimal tongChi = giaoDichs.Where(g => g.LoaiGiaoDich == "Chi").Sum(g => g.SoTien);

            decimal tongSoDu = await _db.TaiKhoans
                .Where(t => t.NguoiDungId == userId && t.TrangThai)
                .SumAsync(t => t.SoDu);

            // ── Biểu đồ cột: Thu/Chi theo danh mục ──────────────────
            var thuTheoDanhMuc = giaoDichs
                .Where(g => g.LoaiGiaoDich == "Thu")
                .GroupBy(g => g.DanhMuc.DanhMucCha != null ? g.DanhMuc.DanhMucCha.TenDanhMuc : g.DanhMuc.TenDanhMuc)
                .Select(x => new { Ten = x.Key, Tong = x.Sum(g => g.SoTien) })
                .OrderByDescending(x => x.Tong)
                .ToList();

            var chiTheoDanhMuc = giaoDichs
                .Where(g => g.LoaiGiaoDich == "Chi")
                .GroupBy(g => g.DanhMuc.DanhMucCha != null ? g.DanhMuc.DanhMucCha.TenDanhMuc : g.DanhMuc.TenDanhMuc)
                .Select(x => new { Ten = x.Key, Tong = x.Sum(g => g.SoTien) })
                .OrderByDescending(x => x.Tong)
                .ToList();

            // Gộp nhãn chung cho biểu đồ cột
            var allLabels = thuTheoDanhMuc.Select(x => x.Ten)
                .Union(chiTheoDanhMuc.Select(x => x.Ten))
                .ToList();

            var thuDict = thuTheoDanhMuc.ToDictionary(x => x.Ten, x => x.Tong);
            var chiDict = chiTheoDanhMuc.ToDictionary(x => x.Ten, x => x.Tong);

            // ── Biểu đồ đường: Dòng tiền tích lũy theo ngày ─────────
            var giaoDichTheoNgay = giaoDichs
                .GroupBy(g => g.NgayGiaoDich)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Ngay = g.Key.ToString("dd/MM"),
                    NetFlow = g.Sum(x => x.LoaiGiaoDich == "Thu" ? x.SoTien : -x.SoTien)
                })
                .ToList();

            decimal cumulative = 0;
            var ngayLabels = new List<string>();
            var soDuTichLuy = new List<decimal>();
            foreach (var d in giaoDichTheoNgay)
            {
                cumulative += d.NetFlow;
                ngayLabels.Add(d.Ngay);
                soDuTichLuy.Add(cumulative);
            }

            // ── Biểu đồ ngang: Số dư tài khoản ──────────────────────
            var taiKhoans = await _db.TaiKhoans
                .Where(t => t.NguoiDungId == userId && t.TrangThai)
                .OrderByDescending(t => t.SoDu)
                .ToListAsync();

            // ── Ngân sách tháng ───────────────────────────────────────
            var nganSachs = await _db.NganSaches
                .Include(n => n.DanhMuc)
                .Where(n => n.NguoiDungId == userId
                         && n.Thang == targetThang
                         && n.Nam == targetNam)
                .OrderByDescending(n => n.SoTienDaChiTieu * 100m / n.SoTienGioiHan)
                .ToListAsync();

            // ── Giao dịch gần đây (10 giao dịch mới nhất) ────────────
            var giaoDichGanDay = await _db.GiaoDiches
                .Include(g => g.DanhMuc)
                .Where(g => g.NguoiDungId == userId)
                .OrderByDescending(g => g.NgayTao)
                .Take(8)
                .ToListAsync();

            // ── Build ViewModel ───────────────────────────────────────
            var vm = new DashboardViewModel
            {
                Thang = targetThang,
                Nam = targetNam,
                TongThu = tongThu,
                TongChi = tongChi,
                TongSoDu = tongSoDu,

                DanhMucLabels = allLabels,
                ThuData = allLabels.Select(l => thuDict.GetValueOrDefault(l, 0)).ToList(),
                ChiData = allLabels.Select(l => chiDict.GetValueOrDefault(l, 0)).ToList(),

                ChiLabels = chiTheoDanhMuc.Select(x => x.Ten).ToList(),
                ChiValues = chiTheoDanhMuc.Select(x => x.Tong).ToList(),

                NgayLabels = ngayLabels,
                SoDuTichLuy = soDuTichLuy,

                TaiKhoanLabels = taiKhoans.Select(t => t.TenTaiKhoan).ToList(),
                TaiKhoanSoDu = taiKhoans.Select(t => t.SoDu).ToList(),

                NganSachItems = nganSachs.Select(n => new NganSachItem
                {
                    TenDanhMuc = n.DanhMuc.TenDanhMuc,
                    GioiHan = n.SoTienGioiHan,
                    DaChiTieu = n.SoTienDaChiTieu,
                    TrangThai = n.SoTienDaChiTieu >= n.SoTienGioiHan
                                    ? "Vượt ngân sách"
                                 : n.SoTienDaChiTieu >= n.SoTienGioiHan * 0.8m
                                    ? "Sắp vượt (≥80%)"
                                    : "Bình thường"
                }).ToList(),

                GiaoDichGanDay = giaoDichGanDay.Select(g => new GiaoDichItem
                {
                    MoTa = g.MoTa ?? g.DanhMuc.TenDanhMuc,
                    TenDanhMuc = g.DanhMuc.TenDanhMuc,
                    LoaiGiaoDich = g.LoaiGiaoDich,
                    SoTien = g.SoTien,
                    NgayGiaoDich = g.NgayGiaoDich.ToString("dd/MM/yyyy"),
                    HinhThuc = g.HinhThucThanhToan ?? ""
                }).ToList()
            };

            // Truyền danh sách năm có dữ liệu để render bộ lọc
            // Lấy danh sách năm: dùng ToList trước rồi xử lý .Year trên client
            var allNgay = await _db.GiaoDiches
                .Where(g => g.NguoiDungId == userId)
                .Select(g => g.NgayGiaoDich)
                .ToListAsync();
            ViewBag.NamList = allNgay
                .Select(d => d.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            return View(vm);
        }
    }
}
