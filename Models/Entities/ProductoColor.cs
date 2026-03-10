using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("ProductoColores")]
public class ProductoColor
{
    [Key]
    public int ProductoColorID { get; set; }

    public int ProductoID { get; set; }
    public int ColorID { get; set; }

    [ForeignKey("ProductoID")]
    public Producto Producto { get; set; } = null!;

    [ForeignKey("ColorID")]
    public Color Color { get; set; } = null!;
}
