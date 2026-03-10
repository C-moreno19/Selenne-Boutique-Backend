using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Colores")]
public class Color
{
    [Key]
    public int ColorID { get; set; }

    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(7)]
    public string? CodigoHex { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } = "activo";

    public ICollection<ProductoColor> ProductoColores { get; set; } = new List<ProductoColor>();
}
