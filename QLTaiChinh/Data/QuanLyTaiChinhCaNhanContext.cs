using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLTaiChinh.Data;

public partial class QuanLyTaiChinhCaNhanContext : DbContext
{
    public QuanLyTaiChinhCaNhanContext()
    {
    }

    public QuanLyTaiChinhCaNhanContext(DbContextOptions<QuanLyTaiChinhCaNhanContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DanhMuc> DanhMucs { get; set; }

    public virtual DbSet<GiaoDich> GiaoDiches { get; set; }

    public virtual DbSet<NganSach> NganSaches { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<PhienDangNhap> PhienDangNhaps { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<VwGiaoDichChiTiet> VwGiaoDichChiTiets { get; set; }

    public virtual DbSet<VwTinhTrangNganSach> VwTinhTrangNganSaches { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Data Source=TUAN-VI\\MSI;Initial Catalog=QuanLyTaiChinhCaNhan;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.DanhMucId).HasName("PK__DanhMuc__1C53BA7BE17F5515");

            entity.ToTable("DanhMuc");

            entity.Property(e => e.DanhMucId).HasColumnName("DanhMucID");
            entity.Property(e => e.DanhMucChaId).HasColumnName("DanhMucChaID");
            entity.Property(e => e.LoaiDanhMuc).HasMaxLength(10);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDungID");
            entity.Property(e => e.TenDanhMuc).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.DanhMucCha).WithMany(p => p.InverseDanhMucCha)
                .HasForeignKey(d => d.DanhMucChaId)
                .HasConstraintName("FK_DanhMuc_DanhMucCha");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.DanhMucs)
                .HasForeignKey(d => d.NguoiDungId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_DanhMuc_NguoiDung");
        });

        modelBuilder.Entity<GiaoDich>(entity =>
        {
            entity.HasKey(e => e.GiaoDichId).HasName("PK__GiaoDich__D8D14B310179CDFB");

            entity.ToTable("GiaoDich", tb =>
                {
                    tb.HasTrigger("trg_CapNhatSoDu_Delete");
                    tb.HasTrigger("trg_CapNhatSoDu_Insert");
                });

            entity.HasIndex(e => e.DanhMucId, "IDX_GiaoDich_DanhMuc");

            entity.HasIndex(e => new { e.NguoiDungId, e.NgayGiaoDich }, "IDX_GiaoDich_NguoiDung_Ngay").IsDescending(false, true);

            entity.HasIndex(e => e.TaiKhoanId, "IDX_GiaoDich_TaiKhoan");

            entity.Property(e => e.GiaoDichId).HasColumnName("GiaoDichID");
            entity.Property(e => e.DanhMucId).HasColumnName("DanhMucID");
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.HinhThucThanhToan).HasMaxLength(50);
            entity.Property(e => e.LoaiGiaoDich).HasMaxLength(10);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayGiaoDich).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDungID");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TaiKhoanId).HasColumnName("TaiKhoanID");

            entity.HasOne(d => d.DanhMuc).WithMany(p => p.GiaoDiches)
                .HasForeignKey(d => d.DanhMucId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GiaoDich_DanhMuc");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.GiaoDiches)
                .HasForeignKey(d => d.NguoiDungId)
                .HasConstraintName("FK_GiaoDich_NguoiDung");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.GiaoDiches)
                .HasForeignKey(d => d.TaiKhoanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GiaoDich_TaiKhoan");
        });

        modelBuilder.Entity<NganSach>(entity =>
        {
            entity.HasKey(e => e.NganSachId).HasName("PK__NganSach__4BD8DF8BD884B5EA");

            entity.ToTable("NganSach");

            entity.HasIndex(e => new { e.NguoiDungId, e.Nam, e.Thang }, "IDX_NganSach_NguoiDung_ThangNam");

            entity.HasIndex(e => new { e.NguoiDungId, e.DanhMucId, e.Thang, e.Nam }, "UQ_NganSach").IsUnique();

            entity.Property(e => e.NganSachId).HasColumnName("NganSachID");
            entity.Property(e => e.DanhMucId).HasColumnName("DanhMucID");
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDungID");
            entity.Property(e => e.SoTienDaChiTieu).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SoTienGioiHan).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.DanhMuc).WithMany(p => p.NganSaches)
                .HasForeignKey(d => d.DanhMucId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NganSach_DanhMuc");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.NganSaches)
                .HasForeignKey(d => d.NguoiDungId)
                .HasConstraintName("FK_NganSach_NguoiDung");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.NguoiDungId).HasName("PK__NguoiDun__C4BBA4DD3000FD71");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D105348EBDB562").IsUnique();

            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDungID");
            entity.Property(e => e.AnhDaiDien).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhauHash).HasMaxLength(255);
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<PhienDangNhap>(entity =>
        {
            entity.HasKey(e => e.PhienId).HasName("PK__PhienDan__F12E178BD04CD757");

            entity.ToTable("PhienDangNhap");

            entity.HasIndex(e => e.Token, "UQ__PhienDan__1EB4F817F92A55F1").IsUnique();

            entity.Property(e => e.PhienId).HasColumnName("PhienID");
            entity.Property(e => e.DiaChiIp)
                .HasMaxLength(50)
                .HasColumnName("DiaChiIP");
            entity.Property(e => e.NgayHetHan).HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDungID");
            entity.Property(e => e.ThietBi).HasMaxLength(200);
            entity.Property(e => e.Token).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.PhienDangNhaps)
                .HasForeignKey(d => d.NguoiDungId)
                .HasConstraintName("FK_PhienDangNhap_NguoiDung");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.TaiKhoanId).HasName("PK__TaiKhoan__9A124B65C30B0C88");

            entity.ToTable("TaiKhoan");

            entity.Property(e => e.TaiKhoanId).HasColumnName("TaiKhoanID");
            entity.Property(e => e.LoaiTaiKhoan).HasMaxLength(50);
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDungID");
            entity.Property(e => e.SoDu).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenTaiKhoan).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.NguoiDungId)
                .HasConstraintName("FK_TaiKhoan_NguoiDung");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.ThongBaoId).HasName("PK__ThongBao__6E51A53B6F7B9105");

            entity.ToTable("ThongBao");

            entity.Property(e => e.ThongBaoId).HasColumnName("ThongBaoID");
            entity.Property(e => e.LoaiThongBao).HasMaxLength(50);
            entity.Property(e => e.NganSachId).HasColumnName("NganSachID");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDungID");
            entity.Property(e => e.NoiDung).HasMaxLength(500);
            entity.Property(e => e.TieuDe).HasMaxLength(200);

            entity.HasOne(d => d.NganSach).WithMany(p => p.ThongBaos)
                .HasForeignKey(d => d.NganSachId)
                .HasConstraintName("FK_ThongBao_NganSach");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.ThongBaos)
                .HasForeignKey(d => d.NguoiDungId)
                .HasConstraintName("FK_ThongBao_NguoiDung");
        });

        modelBuilder.Entity<VwGiaoDichChiTiet>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_GiaoDichChiTiet");

            entity.Property(e => e.GiaoDichId).HasColumnName("GiaoDichID");
            entity.Property(e => e.HinhThucThanhToan).HasMaxLength(50);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.LoaiGiaoDich).HasMaxLength(10);
            entity.Property(e => e.LoaiTaiKhoan).HasMaxLength(50);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.NgayTao).HasColumnType("datetime");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDungID");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenDanhMuc).HasMaxLength(100);
            entity.Property(e => e.TenTaiKhoan).HasMaxLength(100);
        });

        modelBuilder.Entity<VwTinhTrangNganSach>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_TinhTrangNganSach");

            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.NganSachId).HasColumnName("NganSachID");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDungID");
            entity.Property(e => e.PhanTramSuDung).HasColumnType("decimal(5, 1)");
            entity.Property(e => e.SoTienConLai).HasColumnType("decimal(19, 2)");
            entity.Property(e => e.SoTienDaChiTieu).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SoTienGioiHan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenDanhMuc).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasMaxLength(15);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
