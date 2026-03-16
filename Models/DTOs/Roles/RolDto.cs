namespace SelenneApi.Models.DTOs.Roles;

public class RolDto
{
    public int RoleID { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? Estado { get; set; }
    public List<string> Permisos { get; set; } = new();
}

public class PermisoDto
{
    public int PermissionID { get; set; }
    public string Nombre { get; set; } = "";
}

public class UpdateRolDto
{
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public List<int>? PermisoIds { get; set; }
}

public class CreateRolDto
{
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public List<int>? PermisoIds { get; set; }
}