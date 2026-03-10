using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Proveedores")]
public class Proveedor
{
    [Key]
    public int ProveedorID { get; set; }

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Contacto { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Telefono { get; set; }

    [MaxLength(50)]
    public string? Documento { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } = "activo";

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
