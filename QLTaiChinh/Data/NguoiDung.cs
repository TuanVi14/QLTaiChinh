using System;
using System.Collections.Generic;

namespace QLTaiChinh.Data;

public partial class NguoiDung
{
    private string passwordHash;

    public int NguoiDungId { get; set; }

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public string? AnhDaiDien { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? GioiTinh { get; set; }

    public bool TrangThai { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }


    public virtual ICollection<GiaoDich> GiaoDiches { get; set; } = new List<GiaoDich>();

    public virtual ICollection<NganSach> NganSaches { get; set; } = new List<NganSach>();



    public virtual ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();


}
