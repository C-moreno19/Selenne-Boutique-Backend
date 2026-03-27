namespace SelenneApi.Models.DTOs.Usuarios;

public class CreateUsuarioRequestDto
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Documento { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Estado { get; set; }
    public int RoleID { get; set; }
}