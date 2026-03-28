using System;
using System.Collections.Generic;

namespace QLTaiChinh.Data;

public partial class NganSach
{
    public int NganSachId { get; set; }

    public int NguoiDungId { get; set; }

    public int DanhMucId { get; set; }

    public decimal SoTienGioiHan { get; set; }

    public byte Thang { get; set; }

    public short Nam { get; set; }

    public decimal SoTienDaChiTieu { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual DanhMuc DanhMuc { get; set; } = null!;

    public virtual NguoiDung NguoiDung { get; set; } = null!;

}
