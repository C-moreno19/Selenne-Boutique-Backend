using System.ComponentModel.DataAnnotations;

namespace SelenneApi.Models.DTOs.Auth;

public class SignupRequestDto
{
    [Required, MaxLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(9, ErrorMessage = "La contraseña debe tener más de 8 caracteres."), MaxLength(20, ErrorMessage = "La contraseña no puede superar los 20 caracteres."),
     RegularExpression(@"^(?=(.*\d){2})(?=.*[^a-zA-Z0-9\s]).{9,20}$",
         ErrorMessage = "La contraseña debe tener más de 8 caracteres, máximo 20, al menos 2 números y al menos 1 carácter especial.")]
    public string Contrasena { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(20)]
    public string? Documento { get; set; }
}
