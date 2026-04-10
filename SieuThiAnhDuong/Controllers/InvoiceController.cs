using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Data;
using SieuThiAnhDuong.Models;
using System.Security.Claims;

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
        public async Task<IActionResult> Create(List<ChiTietHoaDon> details, string phuongThucThanhToan)
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;
            if (string.IsNullOrEmpty(maNVClaim))
            {
                return RedirectToAction("Login", "Account");
            }
            int idNhanVienDangLogin = int.Parse(maNVClaim);

            if (details == null || !details.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống!";
                return RedirectToAction(nameof(Create));
            }

            // Kiểm tra tồn kho trước khi thanh toán
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

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // --- LOGIC PHÂN CA THEO KHUNG GIỜ 7H - 22H ---
                    var bayGio = DateTime.Now;
                    int gio = bayGio.Hour;
                    int phut = bayGio.Minute;
                    double thoiGianHienTai = gio + (phut / 60.0);

                    string caTruc = "Ngoài giờ làm việc";

                    // Ca 1: 07h00 - 14h30 (7.0 - 14.5)
                    if (thoiGianHienTai >= 7 && thoiGianHienTai < 14.5)
                    {
                        caTruc = "Ca 1 (Sáng)";
                    }
                    // Ca 2: 14h30 - 22h00 (14.5 - 22.0)
                    else if (thoiGianHienTai >= 14.5 && thoiGianHienTai <= 22)
                    {
                        caTruc = "Ca 2 (Chiều)";
                    }

                    var invoice = new HoaDon
                    {
                        NgayLap = bayGio,
                        MaNV = idNhanVienDangLogin,
                        TongTien = details.Sum(x => x.SoLuong * x.DonGia),
                        CaTruc = caTruc,
                        PhuongThucThanhToan = phuongThucThanhToan ?? "Tiền mặt"
                    };

                    _context.HoaDons.Add(invoice);
                    await _context.SaveChangesAsync();

                    foreach (var item in details)
                    {
                        var product = await _context.SanPhams.FindAsync(item.MaSP);
                        if (product != null)
                        {
                            product.SoLuongTon -= item.SoLuong;

                            _context.ChiTietHoaDons.Add(new ChiTietHoaDon
                            {
                                MaHD = invoice.MaHD,
                                MaSP = item.MaSP,
                                SoLuong = item.SoLuong,
                                DonGia = item.DonGia
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction("Print", new { id = invoice.MaHD });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message);
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