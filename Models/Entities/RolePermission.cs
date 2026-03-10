using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("RolePermissions")]
public class RolePermission
{
    [Key]
    public int RolePermissionID { get; set; }

    public int RoleID { get; set; }
    public int PermissionID { get; set; }

    [ForeignKey("RoleID")]
    public Rol Rol { get; set; } = null!;

    [ForeignKey("PermissionID")]
    public Permission Permission { get; set; } = null!;
}
