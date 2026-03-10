using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Productos")]
public class Producto
{
    [Key]
    public int ProductoID { get; set; }

    [Required, MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [MaxLength(500)]
    public string? DescripcionCorta { get; set; }

    public int CategoriaPrincipalID { get; set; }
    public int TipoProductoID { get; set; }
    public int MarcaID { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrecioCompra { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal PrecioVenta { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrecioOferta { get; set; }

    public int Stock { get; set; } = 0;

    public string? ImagenPrincipal { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } = "activo";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime FechaActualizacion { get; set; } = DateTime.Now;

    [ForeignKey("CategoriaPrincipalID")]
    public CategoriaPrincipal Categoria { get; set; } = null!;

    [ForeignKey("TipoProductoID")]
    public TipoProducto TipoProducto { get; set; } = null!;

    [ForeignKey("MarcaID")]
    public Marca Marca { get; set; } = null!;

    public ICollection<ProductoImagen> Imagenes { get; set; } = new List<ProductoImagen>();
    public ICollection<ProductoTalla> ProductoTallas { get; set; } = new List<ProductoTalla>();
    public ICollection<ProductoColor> ProductoColores { get; set; } = new List<ProductoColor>();
    public ICollection<ProductoMaterial> ProductoMateriales { get; set; } = new List<ProductoMaterial>();
    public ICollection<ProductoStockVariante> StockVariantes { get; set; } = new List<ProductoStockVariante>();
    public ICollection<Valoracion> Valoraciones { get; set; } = new List<Valoracion>();
    public ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
}