using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Data;
using SieuThiAnhDuong.Models; // Bắt buộc phải có để gọi class SessionControl
using SieuThiAnhDuong.Models.ViewModels;
using System.Security.Claims;
using System; // Thêm thư viện này để dùng Guid

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
            if (!ModelState.IsValid) return View(model);

            // Tìm user trong DB
            var user = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.Username && u.MatKhau == model.Password);

            if (user != null)
            {
                string chucVuThucTe = user.NhanVien?.ChucVu ?? "Nhân viên";

                // ==========================================
                // [THÊM MỚI] - XỬ LÝ ĐÁ NGƯỜI DÙNG CŨ
                // 1. Tạo mã phiên mới duy nhất cho lần đăng nhập này
                string newSessionId = Guid.NewGuid().ToString();

                // 2. Ghi đè mã phiên mới này vào RAM Server 
                // (Ai đăng nhập sau sẽ chèn mã mới vào, làm mã cũ của người trước bị vô hiệu)
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

                    // [THÊM MỚI] 3. Đưa mã phiên mới vào Cookie của máy tính này
                    new Claim("UserSessionGuid", newSessionId)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Đăng xuất phiên hiện tại của chính máy này (nếu có bị kẹt) trước khi đăng nhập mới
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties { IsPersistent = true });
                TempData["Success"] = "Đăng nhập thành công!";
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
        [IgnoreAntiforgeryToken] // Tránh lỗi bảo mật khi gọi Ajax
        public IActionResult CheckSession()
        {
            // 1. Nếu Cookie đã mất hiệu lực (đã bị SignOut bởi Middleware)
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { status = "stolen" });
            }

            var userName = User.Identity.Name;
            var currentCookieSession = User.FindFirst("UserSessionGuid")?.Value;

            // 2. So sánh mã trong Cookie với mã mới nhất trong RAM
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
    }
}