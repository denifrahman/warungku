using System.ComponentModel.DataAnnotations;

namespace WarungKu.Models
{
    public class Sale
    {
        public int Id { get; set; }

        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;

        public int CustomerId { get; set; }
        public required Customer Customer { get; set; }

        public int UserId { get; set; }

        public int Discount { get; set; }
        public decimal Voucher { get; set; }

        public required User User { get; set; }

        public required String PaymentMethod { get; set; }   

        public required ICollection<SaleItem> Items { get; set; }
    }
}
