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

            // Tìm user trong DB
            var user = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.Username && u.MatKhau == model.Password);

            if (user != null)
            {
                string chucVuThucTe = user.NhanVien?.ChucVu ?? "Nhân viên";

                // TẠO BỘ NHỚ COOKIE CHO USER
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.TenDangNhap),
                    new Claim(ClaimTypes.Role, chucVuThucTe),
                    new Claim("ChucVu", chucVuThucTe),
                    new Claim("FullName", user.NhanVien?.HoTen ?? user.TenDangNhap),
                    new Claim("MaNV", user.MaNV.ToString()),

                    // 👇 DÒNG QUYẾT ĐỊNH ĐÂY: BẮT BUỘC PHẢI THÊM VÀO 👇
                    // Giả sử bảng TaiKhoan của bạn có cột Quyen lưu chữ "Admin"
                    new Claim("Quyen", user.Quyen ?? "Nhân viên")
                };

                // LƯU Ý NHỎ: Nếu Database của bạn không có cột "Quyen" mà bạn phân quyền Admin
                // dựa vào cột "ChucVu" của nhân viên, thì thay dòng trên bằng dòng này:
                // new Claim("Quyen", chucVuThucTe == "Quản trị viên" ? "Admin" : "Nhân viên")

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties { IsPersistent = true });

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