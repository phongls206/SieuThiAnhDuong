using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SieuThiAnhDuong.Models
{
    public class TaiKhoan
    {
        [Key]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        // Bỏ dấu '?' để ép kiểu dữ liệu không được Null trong DB nếu cần
        // Hoặc giữ lại nhưng phải đảm bảo Controller luôn gán giá trị trước khi SaveChanges
        [Display(Name = "Quyền hạn")]
        public string Quyen { get; set; }

        [Display(Name = "Mã nhân viên")]
        public int MaNV { get; set; }

        [ForeignKey("MaNV")]
        public virtual NhanVien? NhanVien { get; set; }
    }
}