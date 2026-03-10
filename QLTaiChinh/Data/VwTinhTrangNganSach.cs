using System;
using System.Collections.Generic;

namespace QLTaiChinh.Data;

public partial class VwTinhTrangNganSach
{
    public int NganSachId { get; set; }

    public int NguoiDungId { get; set; }

    public string HoTen { get; set; } = null!;

    public string TenDanhMuc { get; set; } = null!;

    public byte Thang { get; set; }

    public short Nam { get; set; }

    public decimal SoTienGioiHan { get; set; }

    public decimal SoTienDaChiTieu { get; set; }

    public decimal? SoTienConLai { get; set; }

    public decimal? PhanTramSuDung { get; set; }

    public string TrangThai { get; set; } = null!;
}
