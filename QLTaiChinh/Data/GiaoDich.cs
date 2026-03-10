using System;
using System.Collections.Generic;

namespace QLTaiChinh.Data;

public partial class GiaoDich
{
    public int GiaoDichId { get; set; }

    public int NguoiDungId { get; set; }

    public int TaiKhoanId { get; set; }

    public int DanhMucId { get; set; }

    public string LoaiGiaoDich { get; set; } = null!;

    public decimal SoTien { get; set; }

    public DateOnly NgayGiaoDich { get; set; }

    public string? MoTa { get; set; }

    public string? HinhThucThanhToan { get; set; }

    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual DanhMuc DanhMuc { get; set; } = null!;

    public virtual NguoiDung NguoiDung { get; set; } = null!;

    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
