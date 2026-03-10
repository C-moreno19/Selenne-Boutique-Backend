using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("ProductoMateriales")]
public class ProductoMaterial
{
    [Key]
    public int ProductoMaterialID { get; set; }

    public int ProductoID { get; set; }
    public int MaterialID { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Porcentaje { get; set; }

    [ForeignKey("ProductoID")]
    public Producto Producto { get; set; } = null!;

    [ForeignKey("MaterialID")]
    public Material Material { get; set; } = null!;
}
