using System.ComponentModel.DataAnnotations;

namespace SelenneApi.Models.DTOs.Auth;

public class SignupRequestDto
{
    [Required, MaxLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Contrasena { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }
}
