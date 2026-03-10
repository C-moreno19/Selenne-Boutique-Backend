using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Carrito")]
public class Carrito
{
    [Key]
    public int CarritoID { get; set; }

    public int UsuarioID { get; set; }
    public int ProductoID { get; set; }
    public int Cantidad { get; set; } = 1;

    [MaxLength(10)]
    public string? TallaSeleccionada { get; set; }

    [MaxLength(50)]
    public string? ColorSeleccionado { get; set; }

    public DateTime FechaAgregado { get; set; } = DateTime.Now;

    [ForeignKey("UsuarioID")]
    public Usuario Usuario { get; set; } = null!;

    [ForeignKey("ProductoID")]
    public Producto Producto { get; set; } = null!;
}
