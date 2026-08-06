using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SelenneApi.Models.Entities;

[Table("Materiales")]
public class Material
{
    [Key]
    public int MaterialID { get; set; }
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;
    [MaxLength(20)]
    public string Estado { get; set; } = "activo";
    public ICollection<ProductoMaterial> ProductoMateriales { get; set; } = new List<ProductoMaterial>();
}