using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Data;
using SieuThiAnhDuong.Models;

namespace SieuThiAnhDuong.Controllers
{
    public class UserAccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserAccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Trang danh sách tài khoản (Cập nhật để hỗ trợ Modal thêm mới)
        public async Task<IActionResult> Index()
        {
            var danhSachTaiKhoan = await _context.TaiKhoans
                                         .Include(t => t.NhanVien)
                                         .ToListAsync();

            // Đổ danh sách nhân viên vào ViewBag để Modal ở file Index.cshtml có thể dùng luôn
            ViewBag.MaNV = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.NhanViens, "MaNV", "HoTen");

            return View(danhSachTaiKhoan);
        }

        // 2. Xử lý tạo tài khoản (Dùng cho cả trang Create riêng hoặc Modal)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoan taiKhoan)
        {
            try
            {
                // Kiểm tra nếu Quyen bị trống thì gán mặc định (phòng hờ)
                if (string.IsNullOrEmpty(taiKhoan.Quyen))
                {
                    taiKhoan.Quyen = "Nhân viên";
                }

                // Các bước check trùng MaNV và TenDangNhap giữ nguyên như cũ...

                _context.Add(taiKhoan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cấp tài khoản thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["Error"] = "Lỗi hệ thống: " + innerMsg;
                return RedirectToAction(nameof(Index));
            }
        }
        // 3. Xử lý Reset mật khẩu (Mặc định về 123456)
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan != null)
            {
                taiKhoan.MatKhau = "123456"; // Mật khẩu mặc định mới
                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã reset mật khẩu tài khoản " + id + " về 123456";
            }
            return RedirectToAction(nameof(Index));
        }

        // 4. Xử lý xóa tài khoản
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan != null)
            {
                _context.TaiKhoans.Remove(taiKhoan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa tài khoản thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}