using System.ComponentModel.DataAnnotations;

namespace WarungKu.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Nama Produk wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama tidak boleh melebihi 100 karakter.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Harga wajib diisi.")]
        [Range(1, double.MaxValue, ErrorMessage = "Harga harus lebih dari nol.")] // Harga minimal 1
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok wajib diisi.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok tidak boleh negatif.")] // Stok minimal 0
        public int Stock { get; set; }

    }
}
