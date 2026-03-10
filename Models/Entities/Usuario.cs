using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Usuarios")]
public class Usuario
{
    [Key]
    public int UsuarioID { get; set; }

    [Required, MaxLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(20)]
    public string? Documento { get; set; }

    [MaxLength(255)]
    public string? Direccion { get; set; }

    [MaxLength(100)]
    public string? Ciudad { get; set; }

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public int? RoleID { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } = "activo";

    public bool EmailVerificado { get; set; } = false;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public DateTime? FechaUltimoLogin { get; set; }

    [ForeignKey("RoleID")]
    public Rol? Rol { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
    public ICollection<Carrito> CarritoItems { get; set; } = new List<Carrito>();
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    public ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
}
