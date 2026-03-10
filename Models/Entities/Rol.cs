using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Roles")]
public class Rol
{
    [Key]
    public int RoleID { get; set; }

    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Descripcion { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } = "activo";

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
