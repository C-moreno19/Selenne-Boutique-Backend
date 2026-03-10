using System.ComponentModel.DataAnnotations;

namespace SelenneApi.Models.DTOs.Notificaciones;

public class EnviarNotificacionDto
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
