using System;
using System.Collections.Generic;

namespace QLTaiChinh.Data;

public partial class PhienDangNhap
{
    public int PhienId { get; set; }

    public int NguoiDungId { get; set; }

    public string Token { get; set; } = null!;

    public string? ThietBi { get; set; }

    public string? DiaChiIp { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayHetHan { get; set; }

    public bool TrangThai { get; set; }

    public virtual NguoiDung NguoiDung { get; set; } = null!;
}
