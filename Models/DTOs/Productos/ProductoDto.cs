namespace SelenneApi.Models.DTOs.Productos;

public class ProductoDto
{
    public int ProductoID { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? DescripcionCorta { get; set; }
    public int CategoriaPrincipalID { get; set; }
    public string? CategoriaNombre { get; set; }
    public int TipoProductoID { get; set; }
    public string? TipoNombre { get; set; }
    public int MarcaID { get; set; }
    public string? MarcaNombre { get; set; }
    public decimal? PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal? PrecioOferta { get; set; }
    public int Stock { get; set; }
    public string? ImagenPrincipal { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public List<ImagenDto> Imagenes { get; set; } = new();
    public List<VarianteStockDto> Variantes { get; set; } = new();
    public bool AgotadoGeneral { get; set; } = false;
    public List<TallaStockDto> Tallas { get; set; } = new();
    public List<string> Materiales { get; set; } = new();
    public List<ColorDto> Colores { get; set; } = new();
    public double? PromedioValoracion { get; set; }
    public int TotalValoraciones { get; set; }
}

public class VarianteStockDto
{
    public string? TallaNombre { get; set; }
    public string? ColorNombre { get; set; }
    public int Stock { get; set; }
}

public class SetVariantesDto
{
    public List<VarianteStockDto> Variantes { get; set; } = new();
}

public class ImagenDto
{
    public string URL { get; set; } = string.Empty;
    public string? ColorNombre { get; set; }
}

public class TallaStockDto
{
    public int TallaID { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Stock { get; set; }
}

public class ColorDto
{
    public int ColorID { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? CodigoHex { get; set; }
}