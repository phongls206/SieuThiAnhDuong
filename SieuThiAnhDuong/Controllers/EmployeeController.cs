using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Data;
using SieuThiAnhDuong.Models;

namespace SieuThiAnhDuong.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EmployeeController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.NhanViens.ToListAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVien nhanVien)
        {
            // Loại bỏ các thuộc tính liên kết để không bị lỗi ModelState khi thêm mới
            ModelState.Remove("TaiKhoan");
            ModelState.Remove("HoaDons");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(nhanVien);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Thêm nhân viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Không thể thêm nhân viên. Vui lòng kiểm tra lại dữ liệu!");
                }
            }
            return View(nhanVien);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return NotFound();

            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // FIX: Đã thêm NgaySinh vào danh sách Bind để nhận dữ liệu từ Form gửi về
        public async Task<IActionResult> Edit(int id, [Bind("MaNV,HoTen,GioiTinh,NgaySinh,ChucVu,SoDT,DiaChi")] NhanVien nhanVien)
        {
            if (id != nhanVien.MaNV) return NotFound();

            // Loại bỏ kiểm tra các thuộc tính navigation liên quan để ModelState.IsValid trả về true
            ModelState.Remove("TaiKhoan");
            ModelState.Remove("HoaDons");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhanVien);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Không thể cập nhật thông tin nhân viên!");
                }
            }
            // Nếu có lỗi (ModelState không hợp lệ), quay lại view cùng dữ liệu đã nhập để hiện lỗi validation
            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 1)
            {
                TempData["Error"] = "Cảnh báo: Không thể xóa tài khoản Quản trị viên hệ thống!";
                return RedirectToAction(nameof(Index));
            }

            var nv = await _context.NhanViens.FindAsync(id);
            if (nv != null)
            {
                try
                {
                    _context.NhanViens.Remove(nv);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã xóa nhân viên thành công!";
                }
                catch (Exception)
                {
                    TempData["Error"] = "Không thể xóa nhân viên này vì đang có dữ liệu liên quan!";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}