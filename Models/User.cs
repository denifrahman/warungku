using System.ComponentModel.DataAnnotations;

namespace WarungKu.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        public required string PasswordHash { get; set; } // disimpan hash, bukan plain password

        [Required]
        public string Role { get; set; } = "Cashier"; // default role

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
