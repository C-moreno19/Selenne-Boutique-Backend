using System.ComponentModel.DataAnnotations;

namespace SelenneApi.Models.DTOs.Auth;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
