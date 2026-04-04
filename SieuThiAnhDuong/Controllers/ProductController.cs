using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Data;
using SieuThiAnhDuong.Models;

namespace SieuThiAnhDuong.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách sản phẩm + Tìm kiếm
        public async Task<IActionResult> Index(string searchString)
        {
            var products = from p in _context.SanPhams select p;

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.TenSP.Contains(searchString));
            }

            return View(await products.ToListAsync());
        }

        // GET: Trang thêm sản phẩm mới
        public IActionResult Create()
        {
            return View();
        }

        // POST: Lưu sản phẩm mới vào DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham sanPham)
        {
            try
            {
                // Bỏ qua kiểm tra Valid để tránh lỗi logic với các thuộc tính liên kết
                _context.Add(sanPham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Nếu lỗi (ví dụ lỗi DB), nó sẽ hiển thị thông báo lỗi lên màn hình
                ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + ex.Message);
                return View(sanPham);
            }
        }
        // 1. Mở giao diện Sửa (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null) return NotFound();

            return View(sanPham);
        }

        // 2. Xử lý lưu dữ liệu (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaSP,TenSP,GiaBan,SoLuongTon,DonViTinh")] SanPham sanPham)
        {
            if (id != sanPham.MaSP) return NotFound();

            // "Ép" hệ thống bỏ qua kiểm tra danh sách hóa đơn
            ModelState.Remove("ChiTietHoaDons");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sanPham);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }

            // Nếu vẫn lỗi, nó sẽ hiện chữ đỏ giải thích lý do trên Form
            return View(sanPham);
        }
        // 3. Xử lý Xóa (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham != null)
            {
                _context.SanPhams.Remove(sanPham);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa sản phẩm!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}