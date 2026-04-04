using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Models;

namespace SieuThiAnhDuong.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<SanPham>()
                .Property(p => p.GiaBan)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<HoaDon>()
                .Property(h => h.TongTien)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ChiTietHoaDon>()
                .Property(c => c.DonGia)
                .HasColumnType("decimal(18,2)");
        }

    }
}