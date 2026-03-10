using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Favoritos")]
public class Favorito
{
    [Key]
    public int FavoritoID { get; set; }

    public int UsuarioID { get; set; }
    public int ProductoID { get; set; }

    [MaxLength(500)]
    public string? Nota { get; set; }

    public DateTime FechaAgregado { get; set; } = DateTime.Now;

    [ForeignKey("UsuarioID")]
    public Usuario Usuario { get; set; } = null!;

    [ForeignKey("ProductoID")]
    public Producto Producto { get; set; } = null!;
}
