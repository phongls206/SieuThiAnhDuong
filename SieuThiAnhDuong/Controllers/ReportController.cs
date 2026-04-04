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
       
            var query = _context.HoaDons
                .Include(h => h.NhanVien)
                .Include(h => h.ChiTietHoaDons)
                .AsQueryable();

            
            if (!string.IsNullOrEmpty(searchString))
            {
                
                query = query.Where(h => h.NhanVien.HoTen.Contains(searchString));
            }

            
            if (searchDate.HasValue)
            {
                query = query.Where(h => h.NgayLap.Date == searchDate.Value.Date);
            }

            var hoadons = await query.OrderByDescending(h => h.MaHD).ToListAsync();

            ViewBag.TongDoanhThu = hoadons.Sum(h => h.TongTien);
            ViewBag.SoHoaDon = hoadons.Count;

            var today = DateTime.Today;
            ViewBag.HoaDonHomNay = await _context.HoaDons.CountAsync(h => h.NgayLap.Date == today);
            ViewBag.DoanhThuHomNay = await _context.HoaDons
                .Where(h => h.NgayLap.Date == today)
                .SumAsync(h => (decimal?)h.TongTien) ?? 0;

           
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentDate = searchDate?.ToString("yyyy-MM-dd");

            return View(hoadons);
        }

      
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