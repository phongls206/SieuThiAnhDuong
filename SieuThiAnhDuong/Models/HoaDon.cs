using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SieuThiAnhDuong.Models
{
    public class HoaDon
    {
        [Key]
        public int MaHD { get; set; }
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public decimal TongTien { get; set; }

        [ForeignKey("NhanVien")]
        public int MaNV { get; set; }
        public virtual NhanVien NhanVien { get; set; }

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
    }
}