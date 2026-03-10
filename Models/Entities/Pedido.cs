using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Pedidos")]
public class Pedido
{
    [Key]
    public int PedidoID { get; set; }

    public int ClienteID { get; set; }
    public DateTime FechaPedido { get; set; } = DateTime.Now;

    [Required, MaxLength(100)]
    public string NombreCliente { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? DocumentoCliente { get; set; }

    [Required, MaxLength(100)]
    public string EmailCliente { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string TelefonoCliente { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string DireccionEnvio { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Ciudad { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? CodigoPostal { get; set; }

    [Required, MaxLength(50)]
    public string MetodoPago { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? NumeroCuenta { get; set; }

    [MaxLength(100)]
    public string? NombreTitular { get; set; }

    [MaxLength(100)]
    public string? Banco { get; set; }

    [MaxLength(50)]
    public string? TipoCuenta { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Envio { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } = "Pendiente";

    [MaxLength(50)]
    public string? NumeroGuia { get; set; }

    [MaxLength(100)]
    public string? Transportadora { get; set; }

    public DateTime? FechaEnvio { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public string? Notas { get; set; }
    public DateTime FechaActualizacion { get; set; } = DateTime.Now;

    [ForeignKey("ClienteID")]
    public Usuario Cliente { get; set; } = null!;

    public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
}
