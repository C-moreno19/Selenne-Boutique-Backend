using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Permissions")]
public class Permission
{
    [Key]
    public int PermissionID { get; set; }

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Descripcion { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } = "activo";

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
