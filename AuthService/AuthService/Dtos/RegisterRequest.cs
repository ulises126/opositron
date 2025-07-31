using System.ComponentModel.DataAnnotations;

namespace AuthService.Dtos
{
    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateOnly Birthdate { get; set; }
    }
}
