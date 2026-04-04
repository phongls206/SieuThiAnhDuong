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

        // 1. Danh sách nhân viên
        public async Task<IActionResult> Index() => View(await _context.NhanViens.ToListAsync());

        // 2. Thêm nhân viên (Giao diện)
        public IActionResult Create() => View();

        // 3. Xử lý thêm nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVien nhanVien)
        {
            try
            {
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Không thể thêm nhân viên: " + ex.Message);
                return View(nhanVien);
            }
        }

        // 4. Sửa nhân viên (Mở giao diện - GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return NotFound();

            return View(nhanVien);
        }

        // 5. XỬ LÝ LƯU THAY ĐỔI (PHẦN BỊ THIẾU CỦA BẠN - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNV,HoTen,ChucVu,SoDT,DiaChi")] NhanVien nhanVien)
        {
            if (id != nhanVien.MaNV) return NotFound();

            // Xóa bỏ kiểm tra bắt buộc cho 2 trường đang báo lỗi
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
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }
            return View(nhanVien);
        }
        // 6. Xử lý xóa nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken] // Thêm cho an toàn
        public async Task<IActionResult> Delete(int id)
        {
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv != null)
            {
                _context.NhanViens.Remove(nv);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa nhân viên thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}