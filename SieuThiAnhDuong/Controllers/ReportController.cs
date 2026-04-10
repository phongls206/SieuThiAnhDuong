using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Data;
using SieuThiAnhDuong.Models;
using System.Security.Claims;

namespace SieuThiAnhDuong.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReportController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Monthly(string searchString, DateTime? searchDate, string selectCa, int? selectNV, int page = 1)
        {
            int pageSize = 20; // Giới hạn 20 hóa đơn/trang
            var maNVClaim = User.FindFirst("MaNV")?.Value;
            var quyen = User.FindFirst("Quyen")?.Value;

            // Đổ danh sách nhân viên vào Dropdown cho Admin chọn
            ViewBag.NhanVienList = await _context.NhanViens.OrderBy(n => n.HoTen).ToListAsync();

            var query = _context.HoaDons
                .Include(h => h.NhanVien)
                .AsQueryable();

            // --- LOGIC PHÂN QUYỀN TỐI THƯỢNG ---
            if (quyen == "Admin")
            {
                if (selectNV.HasValue)
                {
                    query = query.Where(h => h.MaNV == selectNV.Value);
                }
                else if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(h => h.NhanVien.HoTen.Contains(searchString));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(maNVClaim))
                {
                    int idHienTai = int.Parse(maNVClaim);
                    query = query.Where(h => h.MaNV == idHienTai);
                }
                else return RedirectToAction("Login", "Account");
            }

            // --- BỘ LỌC CHUNG (Ngày và Ca) ---
            if (searchDate.HasValue)
            {
                query = query.Where(h => h.NgayLap.Date == searchDate.Value.Date);
            }
            if (!string.IsNullOrEmpty(selectCa))
            {
                query = query.Where(h => h.CaTruc == selectCa);
            }

            // --- TÍNH TOÁN THỐNG KÊ (Trước khi phân trang để số liệu chuẩn) ---
            var allFilteredData = await query.ToListAsync();
            ViewBag.SoHoaDon = allFilteredData.Count;
            ViewBag.TongDoanhThu = allFilteredData.Sum(h => h.TongTien);
            ViewBag.TienMat = allFilteredData.Where(h => h.PhuongThucThanhToan == "Tiền mặt").Sum(h => h.TongTien);
            ViewBag.Banking = allFilteredData.Where(h => h.PhuongThucThanhToan == "Chuyển khoản").Sum(h => h.TongTien);

            // --- LOGIC PHÂN TRANG ---
            int totalItems = allFilteredData.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Đảm bảo page không nhỏ hơn 1 hoặc lớn hơn tổng số trang
            page = page < 1 ? 1 : page;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var hoadonsPaging = await query
                .OrderByDescending(h => h.MaHD)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // --- GIỮ TRẠNG THÁI CHO VIEW ---
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentDate = searchDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentCa = selectCa;
            ViewBag.CurrentNV = selectNV;

            return View(hoadonsPaging);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var hoadon = await _context.HoaDons
                .Include(h => h.NhanVien)
                .Include(h => h.ChiTietHoaDons).ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(m => m.MaHD == id);
            return hoadon == null ? NotFound() : View(hoadon);
        }
    }
}