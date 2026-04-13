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

        // Cập nhật searchDate thành string và thêm filterType
        public async Task<IActionResult> Monthly(string searchString, string searchDate, string selectCa, int? selectNV, string filterType, int page = 1)
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
                query = query.Where(h => h.MaHD.ToString().Contains(s) || h.NhanVien.HoTen.Contains(s));
            }

            // --- 3. BỘ LỌC THỜI GIAN LINH HOẠT (Ngày/Tháng/Năm) ---
            if (!string.IsNullOrEmpty(searchDate))
            {
                if (filterType == "year" && int.TryParse(searchDate, out int year))
                {
                    // Lọc theo năm
                    query = query.Where(h => h.NgayLap.Year == year);
                }
                else if (filterType == "month" && DateTime.TryParse(searchDate, out DateTime monthDate))
                {
                    // Lọc theo tháng (chuỗi yyyy-MM)
                    query = query.Where(h => h.NgayLap.Year == monthDate.Year && h.NgayLap.Month == monthDate.Month);
                }
                else if (DateTime.TryParse(searchDate, out DateTime fullDate))
                {
                    // Lọc theo ngày (mặc định hoặc yyyy-MM-dd)
                    query = query.Where(h => h.NgayLap.Date == fullDate.Date);
                }
            }

            // Lọc theo ca trực
            if (!string.IsNullOrEmpty(selectCa))
            {
                query = query.Where(h => h.CaTruc == selectCa);
            }

            // --- 4. TÍNH TOÁN THỐNG KÊ (Tính trên dữ liệu đã lọc nhưng chưa phân trang) ---
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
                .OrderByDescending(h => h.NgayLap) // Sắp xếp theo ngày lập mới nhất
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // --- 6. GIỮ TRẠNG THÁI CHO VIEW ---
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentDate = searchDate;
            ViewBag.FilterType = filterType;
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