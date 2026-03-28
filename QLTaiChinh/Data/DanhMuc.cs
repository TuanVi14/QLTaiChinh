using System;
using System.Collections.Generic;

namespace QLTaiChinh.Data;

public partial class DanhMuc
{
    public int DanhMucId { get; set; }


    public string TenDanhMuc { get; set; } = null!;

    public string LoaiDanhMuc { get; set; } = null!;

    public int? DanhMucChaId { get; set; }

    public bool TrangThai { get; set; }

    public DateTime NgayTao { get; set; }

    public virtual DanhMuc? DanhMucCha { get; set; }

    public virtual ICollection<GiaoDich> GiaoDiches { get; set; } = new List<GiaoDich>();

    public virtual ICollection<DanhMuc> InverseDanhMucCha { get; set; } = new List<DanhMuc>();

    public virtual ICollection<NganSach> NganSaches { get; set; } = new List<NganSach>();

    public virtual NguoiDung? NguoiDung { get; set; }
}
