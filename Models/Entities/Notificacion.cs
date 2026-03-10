using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Notificaciones")]
public class Notificacion
{
    [Key]
    public int NotificacionID { get; set; }

    public int UsuarioID { get; set; }

    [Required, MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    public string Mensaje { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Tipo { get; set; } = "info";

    public bool Leida { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaLeida { get; set; }

    [MaxLength(100)]
    public string? Referencia { get; set; }

    [ForeignKey("UsuarioID")]
    public Usuario Usuario { get; set; } = null!;
}
