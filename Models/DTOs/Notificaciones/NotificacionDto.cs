using System.ComponentModel.DataAnnotations;

namespace SelenneApi.Models.DTOs.Notificaciones;

public class NotificacionDto
{
    public int NotificacionID { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? Referencia { get; set; }
}

public class EnviarNotificacionMasivaDto
{
    [Required, MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    public string Mensaje { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Tipo { get; set; } = "info";

    public List<int>? UsuarioIds { get; set; }
    public bool TodosLosUsuarios { get; set; } = false;
}
