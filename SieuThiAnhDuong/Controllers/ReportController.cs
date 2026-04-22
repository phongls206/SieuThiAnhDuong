using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Data;
using SieuThiAnhDuong.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SieuThiAnhDuong.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReportController(ApplicationDbContext context) => _context = context;

        // Cập nhật tham số để nhận startDate và endDate từ View mới
        public async Task<IActionResult> Monthly(string searchString, DateTime? startDate, DateTime? endDate, string selectCa, int? selectNV)
        {
            // 1. Lấy thông tin user hiện tại từ Claim
            var userClaim = User.FindFirst("MaNV");
            var quyen = User.FindFirst("Quyen")?.Value;

            // 2. Load danh sách nhân viên cho Dropdown (Chỉ Admin mới cần dùng cái này)
            ViewBag.NhanVienList = await _context.NhanViens.OrderBy(n => n.HoTen).ToListAsync();

            var query = _context.HoaDons
                .Include(h => h.NhanVien)
                .AsQueryable();

            // 3. LOGIC PHÂN QUYỀN TRUY CẬP DỮ LIỆU
            if (quyen != "Admin")
            {
                if (userClaim != null && int.TryParse(userClaim.Value, out int idHienTai))
                {
                    // Nhân viên thường chỉ được xem hóa đơn của chính mình
                    query = query.Where(h => h.MaNV == idHienTai);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            else if (selectNV.HasValue)
            {
                // Admin chọn lọc theo một nhân viên cụ thể
                query = query.Where(h => h.MaNV == selectNV.Value);
            }

            // 4. THANH TÌM KIẾM ĐA NĂNG (Mã HD hoặc Tên NV)
            if (!string.IsNullOrEmpty(searchString))
            {
                string s = searchString.Trim();
                // Tìm theo mã hóa đơn hoặc tên nhân viên bán hàng
                query = query.Where(h => h.MaHD.ToString().Contains(s) ||
                                     h.NhanVien.HoTen.Contains(s));
            }

            // 5. BỘ LỌC KHOẢNG THỜI GIAN (TỪ NGÀY - ĐẾN NGÀY)
            // Logic: NgayLap >= StartDate AND NgayLap < (EndDate + 1 day)
            if (startDate.HasValue)
            {
                query = query.Where(h => h.NgayLap >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                // Cộng thêm 1 ngày để bao gồm tất cả giờ phút giây trong ngày kết thúc
                var thựcTếĐếnHếtNgày = endDate.Value.AddDays(1);
                query = query.Where(h => h.NgayLap < thựcTếĐếnHếtNgày);
            }

            // 6. LỌC THEO CA TRỰC
            if (!string.IsNullOrEmpty(selectCa))
            {
                query = query.Where(h => h.CaTruc == selectCa);
            }

            // 7. THỰC THI TRUY VẤN
            // Sắp xếp đơn hàng mới nhất lên trên cùng
            var allData = await query.OrderByDescending(h => h.NgayLap).ToListAsync();

            // 8. TÍNH TOÁN THỐNG KÊ (Dựa trên dữ liệu đã lọc)
            ViewBag.SoHoaDon = allData.Count;
            ViewBag.TongDoanhThu = allData.Sum(h => h.TongTien);
            ViewBag.TienMat = allData.Where(h => h.PhuongThucThanhToan == "Tiền mặt").Sum(h => h.TongTien);
            ViewBag.Banking = allData.Where(h => h.PhuongThucThanhToan == "Chuyển khoản").Sum(h => h.TongTien);

            // 9. GIỮ TRẠNG THÁI GIAO DIỆN (Đổ ngược lại vào Form lọc)
            ViewBag.CurrentSearch = searchString;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentCa = selectCa;
            ViewBag.CurrentNV = selectNV;

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