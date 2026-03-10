using System;
using System.Collections.Generic;

namespace QLTaiChinh.Data;

public partial class TaiKhoan
{
    public int TaiKhoanId { get; set; }

    public int NguoiDungId { get; set; }

    public string TenTaiKhoan { get; set; } = null!;

    public string LoaiTaiKhoan { get; set; } = null!;

    public decimal SoDu { get; set; }

    public bool TrangThai { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual ICollection<GiaoDich> GiaoDiches { get; set; } = new List<GiaoDich>();

    public virtual NguoiDung NguoiDung { get; set; } = null!;
}
