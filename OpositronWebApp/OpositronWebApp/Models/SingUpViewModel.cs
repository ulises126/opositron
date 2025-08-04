using System;
using System.ComponentModel.DataAnnotations;

public class SignUpViewModel
{
    [Required]
    public string Name { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required, DataType(DataType.Password)]
    public string Password { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }
}