using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Marcas")]
public class Marca
{
    [Key]
    public int MarcaID { get; set; }

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Logo { get; set; }

    [MaxLength(200)]
    public string? SitioWeb { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } = "activo";

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
