using System.ComponentModel.DataAnnotations;

namespace SelenneApi.Models.DTOs.Auth;

public class ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NuevaContrasena { get; set; } = string.Empty;
}

public class VerifyEmailDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    [Required]
    public string ContrasenaActual { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NuevaContrasena { get; set; } = string.Empty;
}
