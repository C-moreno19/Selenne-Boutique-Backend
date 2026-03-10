using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("ProductoImagenes")]
public class ProductoImagen
{
    [Key]
    public int ProductoImagenID { get; set; }

    public int ProductoID { get; set; }

    [Required]
    public string URL { get; set; } = string.Empty;

    public int Orden { get; set; } = 0;

    [MaxLength(100)]
    public string? ColorNombre { get; set; }

    [ForeignKey("ProductoID")]
    public Producto Producto { get; set; } = null!;
}