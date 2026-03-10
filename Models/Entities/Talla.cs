using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Tallas")]
public class Talla
{
    [Key]
    public int TallaID { get; set; }

    [Required, MaxLength(10)]
    public string Nombre { get; set; } = string.Empty;

    public int Orden { get; set; } = 0;

    public ICollection<ProductoTalla> ProductoTallas { get; set; } = new List<ProductoTalla>();
}
