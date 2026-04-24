using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SieuThiAnhDuong.Models
{
    public class HoaDon
    {
        [Key]
        public int MaHD { get; set; }

       
        [Display(Name = "Ngày lập")]
        public DateTime NgayLap { get; set; } = DateTime.Now;

       
        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; }

        
        [Display(Name = "Ca trực")]
        public string? CaTruc { get; set; } 

        [Display(Name = "Phương thức thanh toán")]
        public string? PhuongThucThanhToan { get; set; } 
        // ------------------------------------------------

        [ForeignKey("NhanVien")]
        public int MaNV { get; set; }

        public virtual NhanVien? NhanVien { get; set; }

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
    }
}