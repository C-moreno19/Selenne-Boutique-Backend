namespace SelenneApi.Models.DTOs.Usuarios;

public class UsuarioDto
{
    public int UsuarioID { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Documento { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public int? RoleID { get; set; }
    public string? RolNombre { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool EmailVerificado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaUltimoLogin { get; set; }
}

public class UpdateUsuarioRequestDto
{
    public string? NombreCompleto { get; set; }
    public string? Telefono { get; set; }
    public string? Documento { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public int? RoleID { get; set; }
    public string? Estado { get; set; }
}