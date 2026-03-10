using System.ComponentModel.DataAnnotations;

namespace SelenneApi.Models.DTOs.Roles;

public class CreateRolRequestDto
{
    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Descripcion { get; set; }

    public List<int> PermisoIds { get; set; } = new();
}

public class UpdateRolRequestDto
{
    [MaxLength(50)]
    public string? Nombre { get; set; }

    [MaxLength(255)]
    public string? Descripcion { get; set; }

    [MaxLength(20)]
    public string? Estado { get; set; }

    public List<int>? PermisoIds { get; set; }
}

public class AsignarRolDto
{
    [Required]
    public int RoleID { get; set; }
}
