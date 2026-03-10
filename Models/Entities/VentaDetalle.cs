using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("VentaDetalles")]
public class VentaDetalle
{
    [Key]
    public int VentaDetalleID { get; set; }

    public int VentaID { get; set; }
    public int ProductoID { get; set; }
    public int? TallaID { get; set; }
    public int? ColorID { get; set; }
    public int Cantidad { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [ForeignKey("VentaID")]
    public Venta Venta { get; set; } = null!;

    [ForeignKey("ProductoID")]
    public Producto Producto { get; set; } = null!;

    [ForeignKey("TallaID")]
    public Talla? Talla { get; set; }

    [ForeignKey("ColorID")]
    public Color? Color { get; set; }
}
