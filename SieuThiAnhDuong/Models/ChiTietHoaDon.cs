using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SieuThiAnhDuong.Models
{
    public class ChiTietHoaDon
    {
        [Key]
        public int MaCTHD { get; set; }

        [ForeignKey("HoaDon")]
        public int MaHD { get; set; }
        public virtual HoaDon HoaDon { get; set; }

        [ForeignKey("SanPham")]
        public int MaSP { get; set; }
        public virtual SanPham SanPham { get; set; }

        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }
}