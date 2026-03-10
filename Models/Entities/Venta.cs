using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Ventas")]
public class Venta
{
    [Key]
    public int VentaID { get; set; }

    public int? UsuarioID { get; set; }
    public int? ClienteID { get; set; }
    public DateTime FechaVenta { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Envio { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } = "Pendiente";

    [MaxLength(50)]
    public string? MetodoPago { get; set; }

    public string? Notas { get; set; }

    [ForeignKey("UsuarioID")]
    public Usuario? Usuario { get; set; }

    public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
}
