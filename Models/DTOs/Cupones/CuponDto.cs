namespace SelenneApi.Models.DTOs.Cupones;

public class CuponDto
{
    public int CuponID { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string TipoDescuento { get; set; } = string.Empty;
    public decimal ValorDescuento { get; set; }
    public decimal? MontoMinimo { get; set; }
    public int? UsosMaximos { get; set; }
    public int UsosActuales { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CrearCuponDto
{
    public string Codigo { get; set; } = string.Empty;
    public string TipoDescuento { get; set; } = "porcentaje";
    public decimal ValorDescuento { get; set; }
    public decimal? MontoMinimo { get; set; }
    public int? UsosMaximos { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaExpiracion { get; set; }
}

public class ActualizarCuponDto
{
    public decimal? ValorDescuento { get; set; }
    public decimal? MontoMinimo { get; set; }
    public int? UsosMaximos { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public bool? Activo { get; set; }
}

// Version publica de un cupon activo — sin datos internos de uso, para
// mostrar en la tienda (banner de la home, etc.) sin necesitar sesion.
public class CuponPublicoDto
{
    public string Codigo { get; set; } = string.Empty;
    public string TipoDescuento { get; set; } = string.Empty;
    public decimal ValorDescuento { get; set; }
    public decimal? MontoMinimo { get; set; }
    public DateTime? FechaExpiracion { get; set; }
}

public class ValidarCuponDto
{
    public string Codigo { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
}

public class CuponValidoDto
{
    public int CuponID { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string TipoDescuento { get; set; } = string.Empty;
    public decimal ValorDescuento { get; set; }
    public decimal MontoDescuento { get; set; }
}
