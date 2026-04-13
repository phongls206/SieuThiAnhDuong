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
            int pageSize = 20;
            var maNVClaim = User.FindFirst("MaNV")?.Value;
            var quyen = User.FindFirst("Quyen")?.Value;

            // Đổ danh sách nhân viên vào Dropdown cho Admin
            ViewBag.NhanVienList = await _context.NhanViens.OrderBy(n => n.HoTen).ToListAsync();

            var query = _context.HoaDons
                .Include(h => h.NhanVien)
                .AsQueryable();

            // --- 1. LOGIC PHÂN QUYỀN ---
            if (quyen != "Admin")
            {
                if (!string.IsNullOrEmpty(maNVClaim))
                {
                    int idHienTai = int.Parse(maNVClaim);
                    query = query.Where(h => h.MaNV == idHienTai);
                }
                else return RedirectToAction("Login", "Account");
            }
            else if (selectNV.HasValue) // Nếu là Admin và chọn nhân viên cụ thể
            {
                query = query.Where(h => h.MaNV == selectNV.Value);
            }

            // --- 2. THANH TÌM KIẾM ĐA NĂNG (Mã HD hoặc Tên NV) ---
            if (!string.IsNullOrEmpty(searchString))
            {
                string s = searchString.Trim();
                // Tìm theo Mã hóa đơn (nếu là số) HOẶC Tên nhân viên
                query = query.Where(h => h.MaHD.ToString().Contains(s) || h.NhanVien.HoTen.Contains(s));
            }

            // --- 3. BỘ LỌC THEO NGÀY VÀ CA ---
            if (searchDate.HasValue)
            {
                query = query.Where(h => h.NgayLap.Date == searchDate.Value.Date);
            }
            if (!string.IsNullOrEmpty(selectCa))
            {
                query = query.Where(h => h.CaTruc == selectCa);
            }

            // --- 4. TÍNH TOÁN THỐNG KÊ (Trước khi phân trang) ---
            var allFilteredData = await query.ToListAsync();
            ViewBag.SoHoaDon = allFilteredData.Count;
            ViewBag.TongDoanhThu = allFilteredData.Sum(h => h.TongTien);
            ViewBag.TienMat = allFilteredData.Where(h => h.PhuongThucThanhToan == "Tiền mặt").Sum(h => h.TongTien);
            ViewBag.Banking = allFilteredData.Where(h => h.PhuongThucThanhToan == "Chuyển khoản").Sum(h => h.TongTien);

            // --- 5. LOGIC PHÂN TRANG ---
            int totalItems = allFilteredData.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            page = page < 1 ? 1 : page;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var hoadonsPaging = await query
                .OrderByDescending(h => h.MaHD)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // --- 6. GIỮ TRẠNG THÁI CHO VIEW ---
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

            if (hoadon == null) return NotFound();

            return View(hoadon);
        }
    }
}