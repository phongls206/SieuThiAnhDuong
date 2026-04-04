using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Data;
using SieuThiAnhDuong.Models;
using System.Security.Claims; // Cần dòng này để lấy thông tin User

namespace SieuThiAnhDuong.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            ViewBag.Products = _context.SanPhams.OrderBy(p => p.TenSP).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(List<ChiTietHoaDon> details)
        {
            // --- BƯỚC 1: LẤY MANV TỪ NGƯỜI ĐANG ĐĂNG NHẬP (FIX LỖI SAI TÊN) ---
            var maNVClaim = User.FindFirst("MaNV")?.Value;
            if (string.IsNullOrEmpty(maNVClaim))
            {
                return RedirectToAction("Login", "Account");
            }
            int idNhanVienDangLogin = int.Parse(maNVClaim);

            // --- BƯỚC 2: KIỂM TRA GIỎ HÀNG ---
            if (details == null || !details.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống!";
                return RedirectToAction(nameof(Create));
            }

            // --- BƯỚC 3: KIỂM TRA TỒN KHO (CHẶN SỐ ÂM) ---
            foreach (var item in details)
            {
                var product = await _context.SanPhams.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.MaSP == item.MaSP);

                if (product == null || product.SoLuongTon < item.SoLuong)
                {
                    TempData["Error"] = $"Sản phẩm '{product?.TenSP}' không đủ hàng (Còn: {product?.SoLuongTon})";
                    return RedirectToAction(nameof(Create));
                }
            }

            // --- BƯỚC 4: LƯU HÓA ĐƠN ---
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var invoice = new HoaDon
                    {
                        NgayLap = DateTime.Now,
                        MaNV = idNhanVienDangLogin, // Sử dụng ID lấy từ hệ thống, không lấy từ Form
                        TongTien = details.Sum(x => x.SoLuong * x.DonGia)
                    };

                    _context.HoaDons.Add(invoice);
                    await _context.SaveChangesAsync();

                    foreach (var item in details)
                    {
                        var product = await _context.SanPhams.FindAsync(item.MaSP);

                        // Trừ kho thực tế
                        product.SoLuongTon -= item.SoLuong;

                        _context.ChiTietHoaDons.Add(new ChiTietHoaDon
                        {
                            MaHD = invoice.MaHD,
                            MaSP = item.MaSP,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia
                        });
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction("Print", new { id = invoice.MaHD });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                    return RedirectToAction(nameof(Create));
                }
            }
        }

        public async Task<IActionResult> Print(int id)
        {
            var invoice = await _context.HoaDons
                .Include(h => h.NhanVien)
                .Include(h => h.ChiTietHoaDons).ThenInclude(d => d.SanPham)
                .FirstOrDefaultAsync(m => m.MaHD == id);

            if (invoice == null) return NotFound();
            return View(invoice);
        }
    }
}