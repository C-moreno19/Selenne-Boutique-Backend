using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Valoraciones")]
public class Valoracion
{
    [Key]
    public int ValoracionID { get; set; }

    public int ProductoID { get; set; }
    public int UsuarioID { get; set; }
    public int? PedidoID { get; set; }

    [Range(1, 5)]
    public int Puntuacion { get; set; }

    public string? Comentario { get; set; }
    public int Util { get; set; } = 0;
    public int NoUtil { get; set; } = 0;
    public bool VerificadoCompra { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [MaxLength(20)]
    public string Estado { get; set; } = "pendiente";

    [ForeignKey("ProductoID")]
    public Producto Producto { get; set; } = null!;

    [ForeignKey("UsuarioID")]
    public Usuario Usuario { get; set; } = null!;

    [ForeignKey("PedidoID")]
    public Pedido? Pedido { get; set; }
}
