namespace SelenneApi.Models.DTOs.Pedidos;

public class PedidoDto
{
    public int PedidoID { get; set; }
    public int ClienteID { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string EmailCliente { get; set; } = string.Empty;
    public string TelefonoCliente { get; set; } = string.Empty;
    public string DireccionEnvio { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Envio { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? NumeroGuia { get; set; }
    public string? Transportadora { get; set; }
    public DateTime FechaPedido { get; set; }
    public DateTime? FechaEnvio { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public string? Notas { get; set; }
    public List<PedidoDetalleDto> Detalles { get; set; } = new();
}

public class PedidoDetalleDto
{
    public int PedidoDetalleID { get; set; }
    public int ProductoID { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string? ImagenProducto { get; set; }
    public string? Talla { get; set; }
    public string? Color { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
