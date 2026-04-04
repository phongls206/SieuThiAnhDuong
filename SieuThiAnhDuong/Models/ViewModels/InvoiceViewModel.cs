namespace SieuThiAnhDuong.Models.ViewModels
{
    public class InvoiceViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal => Quantity * Price;
    }
}