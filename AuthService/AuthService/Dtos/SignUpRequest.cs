namespace AuthService.Dtos
{
    public class SignUpRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Name { get; set; } = "";
        public DateOnly Birthdate { get; set; }
    }
}
