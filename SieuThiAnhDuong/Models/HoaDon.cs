using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SieuThiAnhDuong.Models
{
    public class HoaDon
    {
        [Key]
        public int MaHD { get; set; }

        // Đảm bảo tên là NgayLap để khớp với Controller của bạn
        [Display(Name = "Ngày lập")]
        public DateTime NgayLap { get; set; } = DateTime.Now;

        // Thêm cột TongTien để hết bị tô đỏ ở Controller
        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; }

        // --- THÊM 2 CỘT MỚI ĐỂ QUẢN LÝ CA VÀ THANH TOÁN ---
        [Display(Name = "Ca trực")]
        public string? CaTruc { get; set; } // Lưu: Ca 1, Ca 2...

        [Display(Name = "Phương thức thanh toán")]
        public string? PhuongThucThanhToan { get; set; } // Lưu: Tiền mặt, Banking
        // ------------------------------------------------

        [ForeignKey("NhanVien")]
        public int MaNV { get; set; }

        public virtual NhanVien? NhanVien { get; set; }

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
    }
}