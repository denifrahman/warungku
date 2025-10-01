using System.ComponentModel.DataAnnotations;

namespace WarungKu.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nama wajib diisi.")]
        public required string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }
}
