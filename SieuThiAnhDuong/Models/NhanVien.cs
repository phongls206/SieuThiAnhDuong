using System.ComponentModel.DataAnnotations;

namespace SieuThiAnhDuong.Models
{
    public class NhanVien
    {
        [Key]
        public int MaNV { get; set; }
        [Required]
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string DiaChi { get; set; }
        public string SoDT { get; set; }
        public string ChucVu { get; set; }

        
        public virtual TaiKhoan? TaiKhoan { get; set; }
        public virtual ICollection<HoaDon>? HoaDons { get; set; }
    }
}