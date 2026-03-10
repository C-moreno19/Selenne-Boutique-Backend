using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("CompraDetalles")]
public class CompraDetalle
{
    [Key]
    public int CompraDetalleID { get; set; }

    public int CompraID { get; set; }
    public int ProductoID { get; set; }
    public int Cantidad { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [MaxLength(50)]
    public string? SKU { get; set; }

    [MaxLength(100)]
    public string? Categoria { get; set; }

    [MaxLength(100)]
    public string? Marca { get; set; }

    [MaxLength(10)]
    public string? Talla { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    [MaxLength(100)]
    public string? Material { get; set; }

    [MaxLength(100)]
    public string? TipoProducto { get; set; }

    [ForeignKey("CompraID")]
    public Compra Compra { get; set; } = null!;

    [ForeignKey("ProductoID")]
    public Producto Producto { get; set; } = null!;
}
