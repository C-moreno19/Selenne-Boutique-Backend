using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("ProductoStockVariantes")]
public class ProductoStockVariante
{
    [Key]
    public int VarianteID { get; set; }

    public int ProductoID { get; set; }

    [MaxLength(100)]
    public string? TallaNombre { get; set; }

    [MaxLength(100)]
    public string? ColorNombre { get; set; }

    public int Stock { get; set; } = 0;

    [ForeignKey("ProductoID")]
    public Producto Producto { get; set; } = null!;
}