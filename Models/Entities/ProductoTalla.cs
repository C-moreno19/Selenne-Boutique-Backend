using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("ProductoTallas")]
public class ProductoTalla
{
    [Key]
    public int ProductoTallaID { get; set; }

    public int ProductoID { get; set; }
    public int TallaID { get; set; }
    public int StockTalla { get; set; } = 0;

    [ForeignKey("ProductoID")]
    public Producto Producto { get; set; } = null!;

    [ForeignKey("TallaID")]
    public Talla Talla { get; set; } = null!;
}
