using System.ComponentModel.DataAnnotations;

namespace SelenneApi.Models.DTOs.Productos;

public class CreateProductoRequestDto
{
    [Required, MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [MaxLength(500)]
    public string? DescripcionCorta { get; set; }

    [Required]
    public int CategoriaPrincipalID { get; set; }

    [Required]
    public int TipoProductoID { get; set; }

    [Required]
    public int MarcaID { get; set; }

    public decimal? PrecioCompra { get; set; }

    [Required, Range(0.01, double.MaxValue)]
    public decimal PrecioVenta { get; set; }

    public decimal? PrecioOferta { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; } = 0;

    public string? ImagenPrincipal { get; set; }
}

public class UpdateProductoRequestDto
{
    [MaxLength(200)]
    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    [MaxLength(500)]
    public string? DescripcionCorta { get; set; }

    public int? CategoriaPrincipalID { get; set; }
    public int? TipoProductoID { get; set; }
    public int? MarcaID { get; set; }
    public decimal? PrecioCompra { get; set; }
    public decimal? PrecioVenta { get; set; }
    public decimal? PrecioOferta { get; set; }

    [Range(0, int.MaxValue)]
    public int? Stock { get; set; }

    public string? ImagenPrincipal { get; set; }

    [MaxLength(20)]
    public string? Estado { get; set; }
}

public class AplicarDescuentoDto
{
    [Required, Range(0, 100)]
    public decimal PorcentajeDescuento { get; set; }
}