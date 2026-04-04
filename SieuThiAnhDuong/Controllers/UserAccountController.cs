using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Data;
using SieuThiAnhDuong.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SieuThiAnhDuong.Controllers
{
    public class UserAccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserAccountController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> Index()
        {
            var danhSachTaiKhoan = await _context.TaiKhoans
                                         .Include(t => t.NhanVien)
                                         .ToListAsync();

           
            var idNhanVienDaCoTK = await _context.TaiKhoans.Select(tk => tk.MaNV).ToListAsync();

            
            var nhanVienChuaCoTK = await _context.NhanViens
                                         .Where(nv => !idNhanVienDaCoTK.Contains(nv.MaNV))
                                         .ToListAsync();

            ViewBag.MaNV = new SelectList(nhanVienChuaCoTK, "MaNV", "HoTen");

            return View(danhSachTaiKhoan);
        }

    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoan taiKhoan)
        {
            try
            {
                
                var nv = await _context.NhanViens.FindAsync(taiKhoan.MaNV);
                if (nv != null)
                {
                    
                    taiKhoan.Quyen = nv.ChucVu;
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy thông tin nhân viên!";
                    return RedirectToAction(nameof(Index));
                }

             
                var exists = await _context.TaiKhoans.AnyAsync(tk => tk.MaNV == taiKhoan.MaNV);
                if (exists)
                {
                    TempData["Error"] = "Nhân viên này đã được cấp tài khoản trước đó rồi!";
                    return RedirectToAction(nameof(Index));
                }

                
                var userExists = await _context.TaiKhoans.AnyAsync(tk => tk.TenDangNhap == taiKhoan.TenDangNhap);
                if (userExists)
                {
                    TempData["Error"] = "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Add(taiKhoan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cấp tài khoản thành công cho nhân viên: " + nv.HoTen;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["Error"] = "Lỗi hệ thống: " + innerMsg;
                return RedirectToAction(nameof(Index));
            }
        }

       
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan != null)
            {
                taiKhoan.MatKhau = "123456";
                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã reset mật khẩu tài khoản " + id + " về 123456";
            }
            return RedirectToAction(nameof(Index));
        }

       
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