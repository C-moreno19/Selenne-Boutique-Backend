using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("StockMovimientos")]
public class StockMovimiento
{
    [Key]
    public int MovimientoID { get; set; }

    public int ProductoID { get; set; }
    public int Cantidad { get; set; }

    [Required, MaxLength(20)]
    public string Tipo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ReferenciaTipo { get; set; }

    public int? ReferenciaID { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int? UsuarioID { get; set; }

    [ForeignKey("ProductoID")]
    public Producto Producto { get; set; } = null!;

    [ForeignKey("UsuarioID")]
    public Usuario? Usuario { get; set; }
}
