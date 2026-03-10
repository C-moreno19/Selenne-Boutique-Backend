using System.ComponentModel.DataAnnotations;

namespace SelenneApi.Models.DTOs.Pedidos;

public class CrearPedidoRequestDto
{
    [Required, MaxLength(100)]
    public string NombreCliente { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? DocumentoCliente { get; set; }

    [Required, EmailAddress]
    public string EmailCliente { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string TelefonoCliente { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string DireccionEnvio { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Ciudad { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? CodigoPostal { get; set; }

    [Required]
    public string MetodoPago { get; set; } = string.Empty;

    public string? NumeroCuenta { get; set; }
    public string? NombreTitular { get; set; }
    public string? Banco { get; set; }
    public string? TipoCuenta { get; set; }
    public string? Notas { get; set; }

    public List<PedidoItemDto> Items { get; set; } = new();
}

public class PedidoItemDto
{
    [Required]
    public int ProductoID { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int Cantidad { get; set; }

    public int? TallaID { get; set; }
    public int? ColorID { get; set; }
}

public class ActualizarEstadoPedidoDto
{
    [Required]
    public string NuevoEstado { get; set; } = string.Empty;
    public string? NumeroGuia { get; set; }
    public string? Transportadora { get; set; }
    public string? Notas { get; set; }
}
