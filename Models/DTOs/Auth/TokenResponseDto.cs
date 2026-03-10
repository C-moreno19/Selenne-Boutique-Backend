namespace SelenneApi.Models.DTOs.Auth;

public class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UsuarioTokenDto Usuario { get; set; } = null!;
}

public class UsuarioTokenDto
{
    public int UsuarioID { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Rol { get; set; }
    public List<string> Permisos { get; set; } = new();
}
