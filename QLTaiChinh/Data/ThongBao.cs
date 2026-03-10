using System;
using System.Collections.Generic;

namespace QLTaiChinh.Data;

public partial class ThongBao
{
    public int ThongBaoId { get; set; }

    public int NguoiDungId { get; set; }

    public int? NganSachId { get; set; }

    public string TieuDe { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public string LoaiThongBao { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public virtual NganSach? NganSach { get; set; }

    public virtual NguoiDung NguoiDung { get; set; } = null!;
}
