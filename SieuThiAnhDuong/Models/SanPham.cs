using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Thêm dòng này

namespace SieuThiAnhDuong.Models
{
    public class SanPham
    {
        [Key]
        public int MaSP { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string TenSP { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải từ 0 trở lên")]
        public decimal GiaBan { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn phải từ 0 trở lên")]
        public int SoLuongTon { get; set; }
        [Required(ErrorMessage = "Đơn vị tính không được để trống ")]
      
        public string? DonViTinh { get; set; }

        [ValidateNever] 
        public virtual ICollection<ChiTietHoaDon>? ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
    }
}