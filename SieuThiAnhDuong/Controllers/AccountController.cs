using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Data;
using SieuThiAnhDuong.Models.ViewModels;
using System.Security.Claims;

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

            var user = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.Username && u.MatKhau == model.Password);

            if (user != null)
            {
                // CHUẨN HÓA ROLE: Đưa Manager/Thủ kho về cùng 1 định dạng để Layout nhận diện đúng
                string userRole = user.Quyen?.Trim() ?? "Nhân viên";
                if (userRole == "Manager") userRole = "Thủ kho"; 

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.TenDangNhap),
                    
                    // Gán Role đã chuẩn hóa
                    new Claim(ClaimTypes.Role, userRole),
                    
                    new Claim("FullName", user.NhanVien?.HoTen ?? user.TenDangNhap),
                    new Claim("MaNV", user.MaNV.ToString())
                };

                // Xóa sạch các Identity cũ trước khi tạo mới để tránh bị "nhồi" nhiều Role
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, 
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                };

                // Đăng xuất mọi phiên làm việc cũ trước khi đăng nhập mới
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

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