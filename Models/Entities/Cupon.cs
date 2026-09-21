using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Cupones")]
public class Cupon
{
    [Key]
    public int CuponID { get; set; }

    [Required, MaxLength(30)]
    public string Codigo { get; set; } = string.Empty;

    // "porcentaje" o "monto"
    [Required, MaxLength(20)]
    public string TipoDescuento { get; set; } = "porcentaje";

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorDescuento { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontoMinimo { get; set; }

    public int? UsosMaximos { get; set; }
    public int UsosActuales { get; set; } = 0;

    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaExpiracion { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
