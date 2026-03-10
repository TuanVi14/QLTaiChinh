using System;
using System.Collections.Generic;

namespace QLTaiChinh.Data;

public partial class VwGiaoDichChiTiet
{
    public int GiaoDichId { get; set; }

    public int NguoiDungId { get; set; }

    public string HoTen { get; set; } = null!;

    public string LoaiGiaoDich { get; set; } = null!;

    public decimal SoTien { get; set; }

    public DateOnly NgayGiaoDich { get; set; }

    public string? MoTa { get; set; }

    public string? HinhThucThanhToan { get; set; }

    public string TenDanhMuc { get; set; } = null!;

    public string TenTaiKhoan { get; set; } = null!;

    public string LoaiTaiKhoan { get; set; } = null!;

    public DateTime NgayTao { get; set; }
}
