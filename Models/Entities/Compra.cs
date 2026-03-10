using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Compras")]
public class Compra
{
    [Key]
    public int CompraID { get; set; }

    public int ProveedorID { get; set; }

    [Required, MaxLength(50)]
    public string OrdenFactura { get; set; } = string.Empty;

    public DateTime Fecha { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } = "Activa";

    public string? Notas { get; set; }

    [ForeignKey("ProveedorID")]
    public Proveedor Proveedor { get; set; } = null!;

    public ICollection<CompraDetalle> Detalles { get; set; } = new List<CompraDetalle>();
}
