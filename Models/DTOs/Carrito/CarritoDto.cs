namespace SelenneApi.Models.DTOs.Carrito;

public class CarritoItemDto
{
    public int CarritoID { get; set; }
    public int ProductoID { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string? ImagenProducto { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal? PrecioOferta { get; set; }
    public int Cantidad { get; set; }
    public string? TallaSeleccionada { get; set; }
    public string? ColorSeleccionado { get; set; }
    public decimal Subtotal { get; set; }
}

public class AgregarCarritoDto
{
    public int ProductoID { get; set; }
    public int Cantidad { get; set; } = 1;
    public string? TallaSeleccionada { get; set; }
    public string? ColorSeleccionado { get; set; }
}

public class ActualizarCantidadDto
{
    public int Cantidad { get; set; }
}
