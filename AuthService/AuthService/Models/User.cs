using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class User
    {
        [Key] // No necesario porque el campo con nombre Id es una clave primaria por convención
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public DateOnly Birthdate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
