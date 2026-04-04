using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SieuThiAnhDuong.Models
{
    public class TaiKhoan
    {
        [Key]
        public string TenDangNhap { get; set; }
        [Required]
        public string MatKhau { get; set; }
        public string? Quyen { get; set; } // Ví dụ: Admin, Staff, Manager

        [ForeignKey("NhanVien")]
        public int MaNV { get; set; }
        public virtual NhanVien NhanVien { get; set; }
    }
}