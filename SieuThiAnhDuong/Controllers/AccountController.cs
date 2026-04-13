using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Data;
using SieuThiAnhDuong.Models;
using SieuThiAnhDuong.Models.ViewModels;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SieuThiAnhDuong.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // ModelState.IsValid vẫn hoạt động vì mình chỉ xóa thuộc tính trong ViewModel, không ảnh hưởng logic
            if (!ModelState.IsValid) return View(model);

            // Tìm user trong DB
            var user = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.Username && u.MatKhau == model.Password);

            if (user != null)
            {
                // Lấy chức vụ thực tế từ bảng Nhân viên
                string chucVuThucTe = user.NhanVien?.ChucVu ?? "Nhân viên";

                // ==========================================
                // XỬ LÝ ĐÁ NGƯỜI DÙNG CŨ (Single Session)
                string newSessionId = Guid.NewGuid().ToString();
                SessionControl.UserSessions[user.TenDangNhap] = newSessionId;
                // ==========================================

                // TẠO BỘ NHỚ COOKIE CHO USER
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.TenDangNhap),
                    new Claim(ClaimTypes.Role, chucVuThucTe),
                    new Claim("ChucVu", chucVuThucTe),
                    new Claim("FullName", user.NhanVien?.HoTen ?? user.TenDangNhap),
                    new Claim("MaNV", user.MaNV.ToString()),
                    new Claim("Quyen", user.Quyen ?? "Nhân viên"),
                    new Claim("UserSessionGuid", newSessionId)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Đăng xuất phiên hiện tại trước khi đăng nhập mới
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // ==========================================
                // [ĐÃ SỬA] BỎ GHI NHỚ ĐĂNG NHẬP
                // IsPersistent = false để đảm bảo an toàn, đóng trình duyệt là hết phiên
                // ==========================================
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties { IsPersistent = false });

                TempData["Success"] = "Đăng nhập thành công!";

                // ==========================================
                // ĐIỀU HƯỚNG THÔNG MINH THEO CHỨC VỤ
                // ==========================================
                if (chucVuThucTe == "Thủ kho")
                {
                    return RedirectToAction("Index", "Product");
                }
                else if (chucVuThucTe == "Admin" || chucVuThucTe == "Nhân viên")
                {
                    return RedirectToAction("Monthly", "Report");
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult CheckSession()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { status = "stolen" });
            }

            var userName = User.Identity.Name;
            var currentCookieSession = User.FindFirst("UserSessionGuid")?.Value;

            if (SessionControl.UserSessions.ContainsKey(userName))
            {
                if (SessionControl.UserSessions[userName] != currentCookieSession)
                {
                    return Json(new { status = "stolen" });
                }
            }

            return Json(new { status = "ok" });
        }

        public async Task<IActionResult> Index()
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;
            if (string.IsNullOrEmpty(maNVClaim)) return RedirectToAction("Login");

            if (int.TryParse(maNVClaim, out int id))
            {
                var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(n => n.MaNV == id);
                if (nhanVien == null) return NotFound();
                return View(nhanVien);
            }
            return RedirectToAction("Login");
        }

        // ==========================================
        // ACTION ĐỔI MẬT KHẨU (Xử lý AJAX từ trang cá nhân)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userName = User.Identity.Name;
            var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.TenDangNhap == userName);

            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            if (user.MatKhau != currentPassword)
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không chính xác." });
            }

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "Xác nhận mật khẩu mới không khớp." });
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 4)
            {
                return Json(new { success = false, message = "Mật khẩu mới phải có ít nhất 4 ký tự." });
            }

            user.MatKhau = newPassword;
            _context.TaiKhoans.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã đổi mật khẩu thành công!" });
        }
    }
}