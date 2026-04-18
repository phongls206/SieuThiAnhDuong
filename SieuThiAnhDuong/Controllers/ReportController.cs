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

        // Bỏ tham số page và pageSize vì DataTables sẽ tự xử lý phân trang ở Client
        public async Task<IActionResult> Monthly(string searchString, string searchDate, string selectCa, int? selectNV, string filterType)
        {
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
                    query = query.Where(h => h.NgayLap.Year == year);
                }
                else if (filterType == "month" && DateTime.TryParse(searchDate, out DateTime monthDate))
                {
                    query = query.Where(h => h.NgayLap.Year == monthDate.Year && h.NgayLap.Month == monthDate.Month);
                }
                else if (DateTime.TryParse(searchDate, out DateTime fullDate))
                {
                    query = query.Where(h => h.NgayLap.Date == fullDate.Date);
                }
            }

            // Lọc theo ca trực
            if (!string.IsNullOrEmpty(selectCa))
            {
                query = query.Where(h => h.CaTruc == selectCa);
            }

            // --- 4. LẤY TOÀN BỘ DỮ LIỆU ĐÃ LỌC ---
            // Sắp xếp mới nhất lên đầu để DataTables hiển thị trang 1 là đơn mới nhất
            var allData = await query.OrderByDescending(h => h.NgayLap).ToListAsync();

            // --- 5. TÍNH TOÁN THỐNG KÊ ---
            ViewBag.SoHoaDon = allData.Count;
            ViewBag.TongDoanhThu = allData.Sum(h => h.TongTien);
            ViewBag.TienMat = allData.Where(h => h.PhuongThucThanhToan == "Tiền mặt").Sum(h => h.TongTien);
            ViewBag.Banking = allData.Where(h => h.PhuongThucThanhToan == "Chuyển khoản").Sum(h => h.TongTien);

            // --- 6. GIỮ TRẠNG THÁI CHO VIEW ---
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentDate = searchDate;
            ViewBag.FilterType = filterType;
            ViewBag.CurrentCa = selectCa;
            ViewBag.CurrentNV = selectNV;

            // Trả về toàn bộ danh sách, DataTables ở View sẽ tự chia trang
            return View(allData);
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