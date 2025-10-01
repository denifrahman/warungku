using System.ComponentModel.DataAnnotations;

namespace WarungKu.Models
{
    public class SaleItem
    {
        public int Id { get; set; }

        [Required]
        public int SaleId { get; set; }
        
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
