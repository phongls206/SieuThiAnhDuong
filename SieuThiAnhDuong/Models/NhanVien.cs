using System.ComponentModel.DataAnnotations;

namespace SieuThiAnhDuong.Models
{
    public class NhanVien
    {
        [Key]
        public int MaNV { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string HoTen { get; set; }



        [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
        [DataType(DataType.Date)]
        // Thêm dòng này để xử lý lỗi sai định dạng/trống
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải có đúng 10 số")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng số 0 và chỉ chứa số")]
        public string SoDT { get; set; }

        [Required(ErrorMessage = "Chức vụ không được để trống")]
        public string ChucVu { get; set; }

        // Các quan hệ liên kết
        public virtual TaiKhoan? TaiKhoan { get; set; }
        public virtual ICollection<HoaDon>? HoaDons { get; set; }
    }
}