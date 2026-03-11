namespace QLTaiChinh.Models
{
    public class DashboardViewModel
    {
        // ── KPI ──────────────────────────────────────────────
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
        public decimal ThangDu => TongThu - TongChi;
        public decimal TongSoDu { get; set; }

        // ── Bộ lọc ───────────────────────────────────────────
        public int Thang { get; set; }
        public int Nam { get; set; }

        // ── Biểu đồ cột: Thu/Chi theo danh mục ───────────────
        public List<string> DanhMucLabels { get; set; } = new();
        public List<decimal> ThuData { get; set; } = new();
        public List<decimal> ChiData { get; set; } = new();

        // ── Biểu đồ tròn: Cơ cấu chi tiêu ───────────────────
        public List<string> ChiLabels { get; set; } = new();
        public List<decimal> ChiValues { get; set; } = new();

        // ── Biểu đồ đường: Dòng tiền theo ngày ──────────────
        public List<string> NgayLabels { get; set; } = new();
        public List<decimal> SoDuTichLuy { get; set; } = new();

        // ── Biểu đồ ngang: Số dư tài khoản ──────────────────
        public List<string> TaiKhoanLabels { get; set; } = new();
        public List<decimal> TaiKhoanSoDu { get; set; } = new();

        // ── Ngân sách ─────────────────────────────────────────
        public List<NganSachItem> NganSachItems { get; set; } = new();

        // ── Giao dịch gần đây ─────────────────────────────────
        public List<GiaoDichItem> GiaoDichGanDay { get; set; } = new();
    }

    public class NganSachItem
    {
        public string TenDanhMuc { get; set; } = "";
        public decimal GioiHan { get; set; }
        public decimal DaChiTieu { get; set; }
        public decimal PhanTram => GioiHan > 0 ? Math.Round(DaChiTieu * 100 / GioiHan, 1) : 0;
        public string TrangThai { get; set; } = "Bình thường"; // "Bình thường" | "Sắp vượt (≥80%)" | "Vượt ngân sách"
    }

    public class GiaoDichItem
    {
        public string MoTa { get; set; } = "";
        public string TenDanhMuc { get; set; } = "";
        public string LoaiGiaoDich { get; set; } = "";
        public decimal SoTien { get; set; }
        public string NgayGiaoDich { get; set; } = "";
        public string HinhThuc { get; set; } = "";
    }
}
