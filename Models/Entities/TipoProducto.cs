using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SelenneApi.Models.Entities;

[Table("TiposProducto")]
public class TipoProducto
{
    [Key]
    public int TipoProductoID { get; set; }
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;
    [MaxLength(20)]
    public string Estado { get; set; } = "activo";
    // Lista separada por comas de categorias principales donde aplica este tipo
    // (ej. "hombre,mujer"). Null o vacio = aplica a todas las categorias.
    [MaxLength(100)]
    public string? CategoriasAplicables { get; set; }
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}