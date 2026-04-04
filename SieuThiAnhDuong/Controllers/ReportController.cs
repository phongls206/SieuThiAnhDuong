using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Data;

namespace SieuThiAnhDuong.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReportController(ApplicationDbContext context) => _context = context;

        // Trang danh sách tổng quát
        public async Task<IActionResult> Monthly(string searchString, DateTime? searchDate)
        {
            // 1. Khởi tạo truy vấn và nạp kèm bảng NhanVien (BẮT BUỘC để hiện tên)
            var query = _context.HoaDons
                .Include(h => h.NhanVien)
                .Include(h => h.ChiTietHoaDons)
                .AsQueryable();

            // 2. Lọc theo tên nhân viên (Tìm kiếm)
            if (!string.IsNullOrEmpty(searchString))
            {
                // Tìm theo HoTen của bảng NhanVien thay vì cột MaNV
                query = query.Where(h => h.NhanVien.HoTen.Contains(searchString));
            }

            // 3. Lọc theo ngày chọn từ Calendar
            if (searchDate.HasValue)
            {
                query = query.Where(h => h.NgayLap.Date == searchDate.Value.Date);
            }

            // Thực thi truy vấn và sắp xếp mới nhất lên đầu
            var hoadons = await query.OrderByDescending(h => h.MaHD).ToListAsync();

            // 4. Tính toán số liệu cho các thẻ Card dựa trên danh sách đã lọc
            ViewBag.TongDoanhThu = hoadons.Sum(h => h.TongTien);
            ViewBag.SoHoaDon = hoadons.Count;

            // Thống kê riêng cho ngày hiện tại (Hôm nay)
            var today = DateTime.Today;
            ViewBag.HoaDonHomNay = await _context.HoaDons.CountAsync(h => h.NgayLap.Date == today);
            ViewBag.DoanhThuHomNay = await _context.HoaDons
                .Where(h => h.NgayLap.Date == today)
                .SumAsync(h => (decimal?)h.TongTien) ?? 0;

            // Truyền searchString và searchDate lại View để giữ giá trị trong ô nhập
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentDate = searchDate?.ToString("yyyy-MM-dd");

            return View(hoadons);
        }

        // Trang hiển thị chi tiết mặt hàng
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hoadon = await _context.HoaDons
                .Include(h => h.NhanVien)
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(m => m.MaHD == id);

            if (hoadon == null) return NotFound();

            return View(hoadon);
        }
    }
}