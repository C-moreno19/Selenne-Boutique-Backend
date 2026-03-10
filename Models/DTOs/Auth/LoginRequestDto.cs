using System.ComponentModel.DataAnnotations;

namespace SelenneApi.Models.DTOs.Auth;

public class LoginRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Contrasena { get; set; } = string.Empty;
}
