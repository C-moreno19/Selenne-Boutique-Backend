using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("CategoriasPrincipales")]
public class CategoriaPrincipal
{
    [Key]
    public int CategoriaPrincipalID { get; set; }

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [MaxLength(500)]
    public string? Imagen { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } = "activo";

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
