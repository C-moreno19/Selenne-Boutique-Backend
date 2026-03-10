namespace SelenneApi.Models.DTOs.Roles;

public class RolDto
{
    public int RoleID { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<string> Permisos { get; set; } = new();
}
