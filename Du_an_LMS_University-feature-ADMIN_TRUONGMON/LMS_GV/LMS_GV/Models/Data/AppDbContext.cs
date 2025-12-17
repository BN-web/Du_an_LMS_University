using System;
using System.Collections.Generic;
using LMS_GV.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_GV.Models.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaiHoc> BaiHocs { get; set; }

    public virtual DbSet<BaiHocLop> BaiHocLops { get; set; }

    public virtual DbSet<BaiKiemTra> BaiKiemTras { get; set; }

    public virtual DbSet<BaiNop> BaiNops { get; set; }

    public virtual DbSet<BaoCao> BaoCaos { get; set; }

    public virtual DbSet<BuoiHoc> BuoiHocs { get; set; }

    public virtual DbSet<BuoiThi> BuoiThis { get; set; }

    public virtual DbSet<CauHoi> CauHois { get; set; }

    public virtual DbSet<ChiTietCauTraLoi> ChiTietCauTraLois { get; set; }

    public virtual DbSet<ChuongTrinhDaoTao> ChuongTrinhDaoTaos { get; set; }

    public virtual DbSet<ChuongTrinhMonHoc> ChuongTrinhMonHocs { get; set; }

    public virtual DbSet<DangKyTinChi> DangKyTinChis { get; set; }

    public virtual DbSet<Diem> Diems { get; set; }

    public virtual DbSet<DiemChuyenCan> DiemChuyenCans { get; set; }

    public virtual DbSet<DiemDanh> DiemDanhs { get; set; }

    public virtual DbSet<DiemDanhChiTiet> DiemDanhChiTiets { get; set; }

    public virtual DbSet<DiemThanhPhan> DiemThanhPhans { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<GiangVien> GiangViens { get; set; }

    public virtual DbSet<GiangVienLop> GiangVienLops { get; set; }

    public virtual DbSet<HoSoSinhVien> HoSoSinhViens { get; set; }

    public virtual DbSet<HocKy> HocKys { get; set; }

    public virtual DbSet<KetQuaKiemTra> KetQuaKiemTras { get; set; }

    public virtual DbSet<Khoa> Khoas { get; set; }

    public virtual DbSet<KhoaTuyenSinh> KhoaTuyenSinhs { get; set; }

    public virtual DbSet<LichSuKien> LichSuKiens { get; set; }

    public virtual DbSet<LichSuSaoLuu> LichSuSaoLuus { get; set; }

    public virtual DbSet<LichSuSuaDiem> LichSuSuaDiems { get; set; }

    public virtual DbSet<LoaiBaoCao> LoaiBaoCaos { get; set; }

    public virtual DbSet<LopHoc> LopHocs { get; set; }

    public virtual DbSet<MonHoc> MonHocs { get; set; }

    public virtual DbSet<Nganh> Nganhs { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NhatKyHeThong> NhatKyHeThongs { get; set; }

    public virtual DbSet<PhongHoc> PhongHocs { get; set; }

    public virtual DbSet<SinhVienLop> SinhVienLops { get; set; }

    public virtual DbSet<ThangDiem> ThangDiems { get; set; }

    public virtual DbSet<ThanhPhanDiem> ThanhPhanDiems { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<ThongBaoNguoiDung> ThongBaoNguoiDungs { get; set; }

    public virtual DbSet<ThongKeHocTap> ThongKeHocTaps { get; set; }

    public virtual DbSet<TienDo> TienDos { get; set; }

    public virtual DbSet<TuyChonCauHoi> TuyChonCauHois { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-BNRH4C1;Database=LMS;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiHoc>(entity =>
        {
            entity.HasKey(e => e.BaiHocId).HasName("PK__BaiHoc__8EFB4A9498445D40");

            entity.ToTable("BaiHoc");

            entity.Property(e => e.BaiHocId).HasColumnName("BaiHoc_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("Created_By");
            entity.Property(e => e.LoaiBaiHoc).HasMaxLength(100);
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.BaiHocs)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_BaiHoc_NguoiDung");
        });

        modelBuilder.Entity<BaiHocLop>(entity =>
        {
            entity.HasKey(e => e.BaiHocLopId).HasName("PK__BaiHoc_L__F66B7A894A6CF5F5");

            entity.ToTable("BaiHoc_Lop");

            entity.Property(e => e.BaiHocLopId).HasColumnName("BaiHoc_Lop_id");
            entity.Property(e => e.BaiHocId).HasColumnName("BaiHoc_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.BaiHoc).WithMany(p => p.BaiHocLops)
                .HasForeignKey(d => d.BaiHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiHocLop_BaiHoc");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.BaiHocLops)
                .HasForeignKey(d => d.LopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiHocLop_LopHoc");
        });

        modelBuilder.Entity<BaiKiemTra>(entity =>
        {
            entity.HasKey(e => e.BaiKiemTraId).HasName("PK__BaiKiemT__5B68F1A56258F755");

            entity.ToTable("BaiKiemTra");

            entity.Property(e => e.BaiKiemTraId).HasColumnName("BaiKiemTra_id");
            entity.Property(e => e.ChoPhepLamLai).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("Created_By");
            entity.Property(e => e.DiemToiDa).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.GiangVienId).HasColumnName("GiangVien_id");
            entity.Property(e => e.IsRandom).HasDefaultValue(false);
            entity.Property(e => e.Loai).HasMaxLength(50);
            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.MonHocId).HasColumnName("MonHoc_id");
            entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.BaiKiemTraCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_BKT_ND");

            entity.HasOne(d => d.GiangVien).WithMany(p => p.BaiKiemTraGiangViens)
                .HasForeignKey(d => d.GiangVienId)
                .HasConstraintName("FK_BKT_GiangVien");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.BaiKiemTras)
                .HasForeignKey(d => d.LopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BKT_Lop");

            entity.HasOne(d => d.MonHoc).WithMany(p => p.BaiKiemTras)
                .HasForeignKey(d => d.MonHocId)
                .HasConstraintName("FK_BKT_MonHoc");
        });

        modelBuilder.Entity<BaiNop>(entity =>
        {
            entity.HasKey(e => e.BaiNopId).HasName("PK__BaiNop__75A01F41AE2E4E50");

            entity.ToTable("BaiNop");

            entity.Property(e => e.BaiNopId).HasColumnName("BaiNop_id");
            entity.Property(e => e.BaiKiemTraId).HasColumnName("BaiKiemTra_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.LanLam).HasDefaultValue(1);
            entity.Property(e => e.NgayNop)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SinhVienId).HasColumnName("SinhVien_id");
            entity.Property(e => e.TongDiem).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.BaiKiemTra).WithMany(p => p.BaiNops)
                .HasForeignKey(d => d.BaiKiemTraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiNop_BaiKiemTra");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.BaiNops)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiNop_SinhVien");
        });

        modelBuilder.Entity<BaoCao>(entity =>
        {
            entity.HasKey(e => e.BaoCaoId).HasName("PK__BaoCao__167F0240ADF4626C");

            entity.ToTable("BaoCao");

            entity.Property(e => e.BaoCaoId).HasColumnName("BaoCao_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("Created_By");
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.LoaiBaoCaoId).HasColumnName("LoaiBaoCao_id");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.BaoCaos)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_BaoCao_NguoiDung");

            entity.HasOne(d => d.LoaiBaoCao).WithMany(p => p.BaoCaos)
                .HasForeignKey(d => d.LoaiBaoCaoId)
                .HasConstraintName("FK_BaoCao_LoaiBaoCao");
        });

        modelBuilder.Entity<BuoiHoc>(entity =>
        {
            entity.HasKey(e => e.BuoiHocId).HasName("PK__BuoiHoc__46A7EF973A8BDAC1");

            entity.ToTable("BuoiHoc");

            entity.Property(e => e.BuoiHocId).HasColumnName("BuoiHoc_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(500)
                .HasColumnName("Ghi_Chu");
            entity.Property(e => e.LoaiBuoiHoc)
                .HasMaxLength(50)
                .HasDefaultValue("giang bai");
            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.PhongHocId).HasColumnName("PhongHoc_id");
            entity.Property(e => e.ThoiGianBatDau).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianKetThuc).HasColumnType("datetime");
            entity.Property(e => e.Thứ).HasMaxLength(50);
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.BuoiHocs)
                .HasForeignKey(d => d.LopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BuoiHoc_LopHoc");

            entity.HasOne(d => d.PhongHoc).WithMany(p => p.BuoiHocs)
                .HasForeignKey(d => d.PhongHocId)
                .HasConstraintName("FK_BuoiHoc_PhongHoc");
        });

        modelBuilder.Entity<BuoiThi>(entity =>
        {
            entity.HasKey(e => e.BuoiThiId).HasName("PK__BuoiThi__4E317803FC4ED95D");

            entity.ToTable("BuoiThi");

            entity.Property(e => e.BuoiThiId).HasColumnName("BuoiThi_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Created_At");
            entity.Property(e => e.GiamThiId).HasColumnName("GiamThi_id");
            entity.Property(e => e.HinhThuc).HasMaxLength(100);
            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.NgayThi).HasColumnType("datetime");
            entity.Property(e => e.PhongHocId).HasColumnName("PhongHoc_id");
            entity.Property(e => e.Thứ).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("Updated_At");

            entity.HasOne(d => d.GiamThi).WithMany(p => p.BuoiThis)
                .HasForeignKey(d => d.GiamThiId)
                .HasConstraintName("FK_BuoiThi_GiamThi");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.BuoiThis)
                .HasForeignKey(d => d.LopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BuoiThi_LopHoc");

            entity.HasOne(d => d.PhongHoc).WithMany(p => p.BuoiThis)
                .HasForeignKey(d => d.PhongHocId)
                .HasConstraintName("FK_BuoiThi_PhongHoc");
        });

        modelBuilder.Entity<CauHoi>(entity =>
        {
            entity.HasKey(e => e.CauHoiId).HasName("PK__CauHoi__7F43422D8A9F9E02");

            entity.ToTable("CauHoi");

            entity.Property(e => e.CauHoiId).HasColumnName("CauHoi_id");
            entity.Property(e => e.BaiKiemTraId).HasColumnName("BaiKiemTra_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Diem).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Loai).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.BaiKiemTra).WithMany(p => p.CauHois)
                .HasForeignKey(d => d.BaiKiemTraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CauHoi_BaiKiemTra");
        });

        modelBuilder.Entity<ChiTietCauTraLoi>(entity =>
        {
            entity.HasKey(e => e.ChiTietCauTraLoiId).HasName("PK__ChiTietC__FDAF2C2DCE77AC6A");

            entity.ToTable("ChiTietCauTraLoi");

            entity.Property(e => e.ChiTietCauTraLoiId).HasColumnName("ChiTietCauTraLoi_id");
            entity.Property(e => e.BaiNopId).HasColumnName("BaiNop_id");
            entity.Property(e => e.CauHoiId).HasColumnName("CauHoi_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Diem).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.LuaChonSinhVien).HasMaxLength(5);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.BaiNop).WithMany(p => p.ChiTietCauTraLois)
                .HasForeignKey(d => d.BaiNopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CT_BN");

            entity.HasOne(d => d.CauHoi).WithMany(p => p.ChiTietCauTraLois)
                .HasForeignKey(d => d.CauHoiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CT_CH");
        });

        modelBuilder.Entity<ChuongTrinhDaoTao>(entity =>
        {
            entity.HasKey(e => e.ChuongTrinhDaoTaoId).HasName("PK__ChuongTr__C895350CD945F520");

            entity.ToTable("ChuongTrinhDaoTao");

            entity.Property(e => e.ChuongTrinhDaoTaoId).HasColumnName("ChuongTrinhDaoTao_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.KhoaTuyenSinhId).HasColumnName("KhoaTuyenSinh_id");
            entity.Property(e => e.NganhId).HasColumnName("Nganh_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.KhoaTuyenSinh).WithMany(p => p.ChuongTrinhDaoTaos)
                .HasForeignKey(d => d.KhoaTuyenSinhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChuongTrinh_KhoaTuyenSinh");

            entity.HasOne(d => d.Nganh).WithMany(p => p.ChuongTrinhDaoTaos)
                .HasForeignKey(d => d.NganhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChuongTrinh_Nganh");
        });

        modelBuilder.Entity<ChuongTrinhMonHoc>(entity =>
        {
            entity.HasKey(e => e.ChuongTrinhMonHocId).HasName("PK__ChuongTr__D16A1D8C85FB1541");

            entity.ToTable("ChuongTrinh_MonHoc");

            entity.Property(e => e.ChuongTrinhMonHocId).HasColumnName("ChuongTrinh_MonHoc_id");
            entity.Property(e => e.ChuongTrinhDaoTaoId).HasColumnName("ChuongTrinhDaoTao_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DaBatBuoc).HasDefaultValue(true);
            entity.Property(e => e.MonHocId).HasColumnName("MonHoc_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ChuongTrinhDaoTao).WithMany(p => p.ChuongTrinhMonHocs)
                .HasForeignKey(d => d.ChuongTrinhDaoTaoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTMonHoc_ChuongTrinh");

            entity.HasOne(d => d.MonHoc).WithMany(p => p.ChuongTrinhMonHocs)
                .HasForeignKey(d => d.MonHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTMonHoc_MonHoc");
        });

        modelBuilder.Entity<DangKyTinChi>(entity =>
        {
            entity.HasKey(e => e.DangKyTinChiId).HasName("PK__DangKyTi__185655A7E4F6AEFE");

            entity.ToTable("DangKyTinChi");

            entity.Property(e => e.DangKyTinChiId).HasColumnName("DangKyTinChi_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.HocKyId).HasColumnName("HocKy_id");
            entity.Property(e => e.MonHocId).HasColumnName("MonHoc_id");
            entity.Property(e => e.NgayDangKy)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SinhVienId).HasColumnName("SinhVien_id");
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.HocKy).WithMany(p => p.DangKyTinChis)
                .HasForeignKey(d => d.HocKyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DangKy_HocKy");

            entity.HasOne(d => d.MonHoc).WithMany(p => p.DangKyTinChis)
                .HasForeignKey(d => d.MonHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DangKy_MonHoc");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.DangKyTinChis)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DangKy_SinhVien");
        });

        modelBuilder.Entity<Diem>(entity =>
        {
            entity.HasKey(e => e.DiemId).HasName("PK__Diem__D39A10B9172BE29A");

            entity.ToTable("Diem");

            entity.Property(e => e.DiemId).HasColumnName("Diem_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DiemChu).HasMaxLength(10);
            entity.Property(e => e.DiemTrungBinhMon).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.Gpamon)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("GPAMon");
            entity.Property(e => e.HeSoMon)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(4, 2)");
            entity.Property(e => e.HocKyId).HasColumnName("HocKy_id");
            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.LyDoRoiMon).HasMaxLength(500);
            entity.Property(e => e.SinhVienId).HasColumnName("SinhVien_id");
            entity.Property(e => e.SoTinChi).HasDefaultValue(3);
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.HocKy).WithMany(p => p.Diems)
                .HasForeignKey(d => d.HocKyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Diem_HocKy");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.Diems)
                .HasForeignKey(d => d.LopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Diem_LopHoc");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.Diems)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Diem_SinhVien");
        });

        modelBuilder.Entity<DiemChuyenCan>(entity =>
        {
            entity.HasKey(e => e.DiemChuyenCanId).HasName("PK__DiemChuy__D1BB0712F7105412");

            entity.ToTable("DiemChuyenCan");

            entity.Property(e => e.DiemChuyenCanId).HasColumnName("DiemChuyenCan_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Diem)
                .HasDefaultValue(10m)
                .HasColumnType("decimal(6, 2)");
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.HocKyId).HasColumnName("HocKy_id");
            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.SinhVienId).HasColumnName("SinhVien_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.HocKy).WithMany(p => p.DiemChuyenCans)
                .HasForeignKey(d => d.HocKyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemChuyenCan_HocKy");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.DiemChuyenCans)
                .HasForeignKey(d => d.LopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemChuyenCan_LopHoc");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.DiemChuyenCans)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemChuyenCan_SinhVien");
        });

        modelBuilder.Entity<DiemDanh>(entity =>
        {
            entity.HasKey(e => e.DiemDanhId).HasName("PK__DiemDanh__FE467F7042FA59B2");

            entity.ToTable("DiemDanh");

            entity.Property(e => e.DiemDanhId).HasColumnName("DiemDanh_id");
            entity.Property(e => e.BuoiHocId).HasColumnName("BuoiHoc_id");
            entity.Property(e => e.BuoiThiId).HasColumnName("BuoiThi_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.Ngay)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.BuoiHoc).WithMany(p => p.DiemDanhs)
                .HasForeignKey(d => d.BuoiHocId)
                .HasConstraintName("FK_DiemDanh_BuoiHoc");

            entity.HasOne(d => d.BuoiThi).WithMany(p => p.DiemDanhs)
                .HasForeignKey(d => d.BuoiThiId)
                .HasConstraintName("FK_DiemDanh_BuoiThi");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.DiemDanhs)
                .HasForeignKey(d => d.LopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemDanh_LopHoc");
        });

        modelBuilder.Entity<DiemDanhChiTiet>(entity =>
        {
            entity.HasKey(e => e.DiemDanhChiTietId).HasName("PK__DiemDanh__148B221F64243C00");

            entity.ToTable("DiemDanh_ChiTiet");

            entity.Property(e => e.DiemDanhChiTietId).HasColumnName("DiemDanh_ChiTiet_id");
            entity.Property(e => e.DiemDanhId).HasColumnName("DiemDanh_id");
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.SinhVienId).HasColumnName("SinhVien_id");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("co mat");

            entity.HasOne(d => d.DiemDanh).WithMany(p => p.DiemDanhChiTiets)
                .HasForeignKey(d => d.DiemDanhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemDanhChiTiet_DiemDanh");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.DiemDanhChiTiets)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemDanhChiTiet_SinhVien");
        });

        modelBuilder.Entity<DiemThanhPhan>(entity =>
        {
            entity.HasKey(e => e.DiemThanhPhanId).HasName("PK__DiemThan__33807429CE8CC295");

            entity.ToTable("DiemThanhPhan");

            entity.Property(e => e.DiemThanhPhanId).HasColumnName("DiemThanhPhan_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Diem).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.SinhVienId).HasColumnName("SinhVien_id");
            entity.Property(e => e.ThanhPhanDiemId).HasColumnName("ThanhPhanDiem_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.DiemThanhPhans)
                .HasForeignKey(d => d.LopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemThanhPhan_LopHoc");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.DiemThanhPhans)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemThanhPhan_SinhVien");

            entity.HasOne(d => d.ThanhPhanDiem).WithMany(p => p.DiemThanhPhans)
                .HasForeignKey(d => d.ThanhPhanDiemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemThanhPhan_ThanhPhan");
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.HasKey(e => e.FilesId).HasName("PK__Files__4C9E1D91DA5FA2D0");

            entity.Property(e => e.FilesId).HasColumnName("Files_id");
            entity.Property(e => e.BaiHocId).HasColumnName("BaiHoc_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DuongDan).HasMaxLength(500);
            entity.Property(e => e.TenFile).HasMaxLength(255);

            entity.HasOne(d => d.BaiHoc).WithMany(p => p.Files)
                .HasForeignKey(d => d.BaiHocId)
                .HasConstraintName("FK_Files_BaiHoc");
        });

        modelBuilder.Entity<GiangVien>(entity =>
        {
            entity.HasKey(e => e.GiangVienId).HasName("PK__GiangVie__DB18C21882EE69F3");

            entity.ToTable("GiangVien");

            entity.Property(e => e.GiangVienId).HasColumnName("GiangVien_id");
            entity.Property(e => e.ChucVu).HasMaxLength(100);
            entity.Property(e => e.ChuyenMon).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.HocVi).HasMaxLength(100);
            entity.Property(e => e.KhoaId).HasColumnName("Khoa_id");
            entity.Property(e => e.Ms)
                .HasMaxLength(10)
                .HasColumnName("MS");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDung_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.GiangViens)
                .HasForeignKey(d => d.NguoiDungId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GiangVien_NguoiDung");
        });

        modelBuilder.Entity<GiangVienLop>(entity =>
        {
            entity.HasKey(e => e.GiangVienLopId).HasName("PK__GiangVie__1E3980D5AB2900E2");

            entity.ToTable("GiangVien_Lop");

            entity.Property(e => e.GiangVienLopId).HasColumnName("GiangVien_Lop_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GiangVienId).HasColumnName("GiangVien_id");
            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.NgayPhanCong)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenGiangVien).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.GiangVien).WithMany(p => p.GiangVienLops)
                .HasForeignKey(d => d.GiangVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GiangVienLop_GiangVien");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.GiangVienLops)
                .HasForeignKey(d => d.LopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GiangVienLop_LopHoc");
        });

        modelBuilder.Entity<HoSoSinhVien>(entity =>
        {
            entity.HasKey(e => e.SinhVienId).HasName("PK__HoSoSinh__C35C4E5244A2E1A0");

            entity.ToTable("HoSoSinhVien");

            entity.Property(e => e.SinhVienId).HasColumnName("SinhVien_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Gpa)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("GPA");
            entity.Property(e => e.KhoaTuyenSinhId).HasColumnName("KhoaTuyenSinh_id");
            entity.Property(e => e.Mssv)
                .HasMaxLength(50)
                .HasColumnName("MSSV");
            entity.Property(e => e.NganhId).HasColumnName("Nganh_id");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDung_id");
            entity.Property(e => e.ThoiGianDaoTao).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.HoSoSinhViens)
                .HasForeignKey(d => d.NguoiDungId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HoSoSinhVien_NguoiDung");

            entity.HasOne(d => d.Nganh).WithMany()
                .HasForeignKey(d => d.NganhId)
                .HasConstraintName("FK_HoSoSinhVien_Nganh");

            entity.HasOne(d => d.KhoaTuyenSinh).WithMany()
                .HasForeignKey(d => d.KhoaTuyenSinhId)
                .HasConstraintName("FK_HoSoSinhVien_KhoaTuyenSinh");
        });

        modelBuilder.Entity<HocKy>(entity =>
        {
            entity.HasKey(e => e.HocKyId).HasName("PK__HocKy__9CCEF84F2D67E86A");

            entity.ToTable("HocKy");

            entity.Property(e => e.HocKyId).HasColumnName("HocKy_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.KiHoc).HasMaxLength(20);
            entity.Property(e => e.NamHoc).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<KetQuaKiemTra>(entity =>
        {
            entity.HasKey(e => e.KetQuaKiemTraId).HasName("PK__KetQuaKi__4E3CD73DE10687BB");

            entity.ToTable("KetQuaKiemTra");

            entity.Property(e => e.KetQuaKiemTraId).HasColumnName("KetQuaKiemTra_id");
            entity.Property(e => e.BaiKiemTraId).HasColumnName("BaiKiemTra_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsAutoGraded)
                .HasDefaultValue(true)
                .HasColumnName("Is_AutoGraded");
            entity.Property(e => e.SinhVienId).HasColumnName("SinhVien_id");
            entity.Property(e => e.TongDiem).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.BaiKiemTra).WithMany(p => p.KetQuaKiemTras)
                .HasForeignKey(d => d.BaiKiemTraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KQ_BaiKiemTra");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.KetQuaKiemTras)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KQ_SinhVien");
        });

        modelBuilder.Entity<Khoa>(entity =>
        {
            entity.HasKey(e => e.KhoaId).HasName("PK__Khoa__980C40FA93063B45");

            entity.ToTable("Khoa");

            entity.Property(e => e.KhoaId).HasColumnName("Khoa_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MaKhoa).HasMaxLength(10);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenKhoa).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<KhoaTuyenSinh>(entity =>
        {
            entity.HasKey(e => e.KhoaTuyenSinhId).HasName("PK__KhoaTuye__3323E777FFB7274B");

            entity.ToTable("KhoaTuyenSinh");

            entity.Property(e => e.KhoaTuyenSinhId).HasColumnName("KhoaTuyenSinh_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MaKhoaTuyenSinh).HasMaxLength(10);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenKhoaTuyenSinh).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<LichSuKien>(entity =>
        {
            entity.HasKey(e => e.LichSuKienId).HasName("PK__LichSuKi__DB1159F9B873CC24");

            entity.ToTable("LichSuKien");

            entity.Property(e => e.LichSuKienId).HasColumnName("LichSuKien_id");
            entity.Property(e => e.BuoiHocId).HasColumnName("BuoiHoc_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Loai).HasMaxLength(100);
            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.NguoiTaoId).HasColumnName("NguoiTao_id");
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.BuoiHoc).WithMany(p => p.LichSuKiens)
                .HasForeignKey(d => d.BuoiHocId)
                .HasConstraintName("FK_LichSuKien_BuoiHoc");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.LichSuKiens)
                .HasForeignKey(d => d.LopHocId)
                .HasConstraintName("FK_LichSuKien_LopHoc");

            entity.HasOne(d => d.NguoiTao).WithMany(p => p.LichSuKiens)
                .HasForeignKey(d => d.NguoiTaoId)
                .HasConstraintName("FK_LichSuKien_NguoiDung");
        });

        modelBuilder.Entity<LichSuSaoLuu>(entity =>
        {
            entity.HasKey(e => e.LichSuSaoLuuId).HasName("PK__LichSuSa__95A3289B87CFDDF9");

            entity.ToTable("LichSuSaoLuu");

            entity.Property(e => e.LichSuSaoLuuId).HasColumnName("LichSuSaoLuu_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DuongDan).HasMaxLength(500);
            entity.Property(e => e.GhiChu).HasMaxLength(500);
        });

        modelBuilder.Entity<LichSuSuaDiem>(entity =>
        {
            entity.HasKey(e => e.LichSuSuaDiemId).HasName("PK__LichSuSu__F12F710D6384A2B3");

            entity.ToTable("LichSuSuaDiem");

            entity.Property(e => e.LichSuSuaDiemId).HasColumnName("LichSuSuaDiem_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DiemId).HasColumnName("Diem_id");
            entity.Property(e => e.DiemLucDau).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.DiemMoi).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.LyDo).HasMaxLength(500);
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDung_id");

            entity.HasOne(d => d.Diem).WithMany(p => p.LichSuSuaDiems)
                .HasForeignKey(d => d.DiemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichSuSuaDiem_Diem");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.LichSuSuaDiems)
                .HasForeignKey(d => d.NguoiDungId)
                .HasConstraintName("FK_LichSuSuaDiem_NguoiDung");
        });

        modelBuilder.Entity<LoaiBaoCao>(entity =>
        {
            entity.HasKey(e => e.LoaiBaoCaoId).HasName("PK__LoaiBaoC__F0D6D0858B3B2E57");

            entity.ToTable("LoaiBaoCao");

            entity.Property(e => e.LoaiBaoCaoId).HasColumnName("LoaiBaoCao_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenLoai).HasMaxLength(255);
        });

        modelBuilder.Entity<LopHoc>(entity =>
        {
            entity.HasKey(e => e.LopHocId).HasName("PK__LopHoc__02BA6DFB8DE10223");

            entity.ToTable("LopHoc");

            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GiangVienId).HasColumnName("GiangVien_id");
            entity.Property(e => e.HocKyId).HasColumnName("HocKy_id");
            entity.Property(e => e.KhoaId).HasColumnName("Khoa_id");
            entity.Property(e => e.KhoaTuyenSinhId).HasColumnName("KhoaTuyenSinh_id");
            entity.Property(e => e.MaLop).HasMaxLength(10);
            entity.Property(e => e.MonHocId).HasColumnName("MonHoc_id");
            entity.Property(e => e.NganhId).HasColumnName("Nganh_id");
            entity.Property(e => e.TenLop).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.GiangVien).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.GiangVienId)
                .HasConstraintName("FK_LopHoc_GiangVien");

            entity.HasOne(d => d.HocKy).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.HocKyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LopHoc_HocKy");

            entity.HasOne(d => d.Khoa).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.KhoaId)
                .HasConstraintName("FK_LopHoc_Khoa");

            entity.HasOne(d => d.KhoaTuyenSinh).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.KhoaTuyenSinhId)
                .HasConstraintName("FK_LopHoc_KhoaTuyenSinh");

            entity.HasOne(d => d.MonHoc).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.MonHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LopHoc_MonHoc");

            entity.HasOne(d => d.Nganh).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.NganhId)
                .HasConstraintName("FK_LopHoc_Nganh");
        });

        modelBuilder.Entity<MonHoc>(entity =>
        {
            entity.HasKey(e => e.MonHocId).HasName("PK__MonHoc__A9D30D888F79204E");

            entity.ToTable("MonHoc");

            entity.Property(e => e.MonHocId).HasColumnName("MonHoc_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MaMon).HasMaxLength(10);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.SoTinChi).HasDefaultValue(0);
            entity.Property(e => e.TenMon).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Nganh>(entity =>
        {
            entity.HasKey(e => e.NganhId).HasName("PK__Nganh__874AF6E7DD28AFD3");

            entity.ToTable("Nganh");

            entity.Property(e => e.NganhId).HasColumnName("Nganh_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.KhoaId).HasColumnName("Khoa_id");
            entity.Property(e => e.MaNganh).HasMaxLength(10);
            entity.Property(e => e.TenNganh).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Khoa).WithMany(p => p.Nganhs)
                .HasForeignKey(d => d.KhoaId)
                .HasConstraintName("FK_Nganh_Khoa");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.NguoiDungId).HasName("PK__NguoiDun__12629C268038EB46");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.TenDangNhap, "UQ__NguoiDun__55F68FC0F0425C9E").IsUnique();

            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDung_id");
            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DangnhapGoogle).HasMaxLength(255);
            entity.Property(e => e.DiaChi).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HashMatKhau)
                .HasMaxLength(255)
                .HasColumnName("Hash_MatKhau");
            entity.Property(e => e.HoTen).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.TenDangNhap).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VaiTroId).HasColumnName("VaiTro_id");

            entity.HasOne(d => d.VaiTro).WithMany(p => p.NguoiDungs)
                .HasForeignKey(d => d.VaiTroId)
                .HasConstraintName("FK_NguoiDung_VaiTro");
        });

        modelBuilder.Entity<NhatKyHeThong>(entity =>
        {
            entity.HasKey(e => e.NhatKyHeThongId).HasName("PK__NhatKyHe__8C153D27028B2BD8");

            entity.ToTable("NhatKyHeThong");

            entity.Property(e => e.NhatKyHeThongId).HasColumnName("NhatKyHeThong_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.HanhDong).HasMaxLength(500);
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDung_id");
            entity.Property(e => e.ThamChieu).HasMaxLength(200);

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.NhatKyHeThongs)
                .HasForeignKey(d => d.NguoiDungId)
                .HasConstraintName("FK_NhatKyHeThong_NguoiDung");
        });

        modelBuilder.Entity<PhongHoc>(entity =>
        {
            entity.HasKey(e => e.PhongHocId).HasName("PK__PhongHoc__E74FDCE293487A0B");

            entity.ToTable("PhongHoc");

            entity.Property(e => e.PhongHocId).HasColumnName("PhongHoc_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.MaPhong).HasMaxLength(50);
            entity.Property(e => e.TenPhong).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<SinhVienLop>(entity =>
        {
            entity.HasKey(e => e.SinhVienLopId).HasName("PK__SinhVien__1F120DC0840EFE27");

            entity.ToTable("SinhVien_Lop");

            entity.Property(e => e.SinhVienLopId).HasColumnName("SinhVien_Lop_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.NgayVao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SinhVienId).HasColumnName("SinhVien_id");
            entity.Property(e => e.TinhTrang)
                .HasMaxLength(50)
                .HasDefaultValue("dang hoc");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.SinhVienLops)
                .HasForeignKey(d => d.LopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SinhVienLop_LopHoc");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.SinhVienLops)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SinhVienLop_SinhVien");
        });

        modelBuilder.Entity<ThangDiem>(entity =>
        {
            entity.HasKey(e => e.ThangDiemId).HasName("PK__ThangDie__0F1B74166CA4F625");

            entity.ToTable("ThangDiem");

            entity.Property(e => e.ThangDiemId).HasColumnName("ThangDiem_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DiemChu).HasMaxLength(10);
            entity.Property(e => e.DiemMax)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("Diem_Max");
            entity.Property(e => e.DiemMin)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("Diem_Min");
        });

        modelBuilder.Entity<ThanhPhanDiem>(entity =>
        {
            entity.HasKey(e => e.ThanhPhanDiemId).HasName("PK__ThanhPha__F94A08CE49924AD0");

            entity.ToTable("ThanhPhanDiem");

            entity.Property(e => e.ThanhPhanDiemId).HasColumnName("ThanhPhanDiem_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.HeSo)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.Ten).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.ThanhPhanDiems)
                .HasForeignKey(d => d.LopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThanhPhan_LopHoc");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.ThongBaoId).HasName("PK__ThongBao__EBC22060F450D392");

            entity.ToTable("ThongBao");

            entity.Property(e => e.ThongBaoId).HasColumnName("ThongBao_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguoiDangId).HasColumnName("NguoiDang_id");
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.NguoiDang).WithMany(p => p.ThongBaos)
                .HasForeignKey(d => d.NguoiDangId)
                .HasConstraintName("FK_ThongBao_NguoiDung");
        });

        modelBuilder.Entity<ThongBaoNguoiDung>(entity =>
        {
            entity.HasKey(e => e.ThongBaoNguoiDungId).HasName("PK__ThongBao__578D7EAC2A13F95A");

            entity.ToTable("ThongBao_NguoiDung");

            entity.Property(e => e.ThongBaoNguoiDungId).HasColumnName("ThongBao_NguoiDung_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DaDoc).HasDefaultValue(false);
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDung_id");
            entity.Property(e => e.ThongBaoId).HasColumnName("ThongBao_id");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.ThongBaoNguoiDungs)
                .HasForeignKey(d => d.NguoiDungId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBND_NguoiDung");

            entity.HasOne(d => d.ThongBao).WithMany(p => p.ThongBaoNguoiDungs)
                .HasForeignKey(d => d.ThongBaoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBND_ThongBao");
        });

        modelBuilder.Entity<ThongKeHocTap>(entity =>
        {
            entity.HasKey(e => e.ThongKeHocTapId).HasName("PK__ThongKeH__3FA6C0DB867431EB");

            entity.ToTable("ThongKeHocTap");

            entity.Property(e => e.ThongKeHocTapId).HasColumnName("ThongKeHocTap_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.HocKyId).HasColumnName("HocKy_id");
            entity.Property(e => e.LopHocId).HasColumnName("LopHoc_id");
            entity.Property(e => e.SinhVienId).HasColumnName("SinhVien_id");
            entity.Property(e => e.Ten).HasMaxLength(255);

            entity.HasOne(d => d.HocKy).WithMany(p => p.ThongKeHocTaps)
                .HasForeignKey(d => d.HocKyId)
                .HasConstraintName("FK_ThongKe_HocKy");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.ThongKeHocTaps)
                .HasForeignKey(d => d.LopHocId)
                .HasConstraintName("FK_ThongKe_LopHoc");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.ThongKeHocTaps)
                .HasForeignKey(d => d.SinhVienId)
                .HasConstraintName("FK_ThongKe_SinhVien");
        });

        modelBuilder.Entity<TienDo>(entity =>
        {
            entity.HasKey(e => e.TienDoId).HasName("PK__TienDo__4412607AD30073CD");

            entity.ToTable("TienDo");

            entity.Property(e => e.TienDoId).HasColumnName("TienDo_id");
            entity.Property(e => e.BaiKiemTraId).HasColumnName("BaiKiemTra_id");
            entity.Property(e => e.SinhVienId).HasColumnName("SinhVien_id");
            entity.Property(e => e.SoCauDaLam).HasDefaultValue(0);
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)0);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Updated_At");

            entity.HasOne(d => d.BaiKiemTra).WithMany(p => p.TienDos)
                .HasForeignKey(d => d.BaiKiemTraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TienDo_BaiKiemTra");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.TienDos)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TienDo_SinhVien");
        });

        modelBuilder.Entity<TuyChonCauHoi>(entity =>
        {
            entity.HasKey(e => e.TuyChonCauHoiId).HasName("PK__TuyChonC__895AD21A1F92AFC8");

            entity.ToTable("TuyChonCauHoi");

            entity.Property(e => e.TuyChonCauHoiId).HasColumnName("TuyChonCauHoi_id");
            entity.Property(e => e.CauHoiId).HasColumnName("CauHoi_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MaLuaChon).HasMaxLength(5);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CauHoi).WithMany(p => p.TuyChonCauHois)
                .HasForeignKey(d => d.CauHoiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TuyChonCauHoi_CauHoi");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.VaiTroId).HasName("PK__VaiTro__E6B13929A6FC0D29");

            entity.ToTable("VaiTro");

            entity.Property(e => e.VaiTroId)
                .ValueGeneratedNever()
                .HasColumnName("VaiTro_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenVaiTro).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
