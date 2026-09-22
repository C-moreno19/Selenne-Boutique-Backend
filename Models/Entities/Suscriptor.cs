using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("Suscriptores")]
public class Suscriptor
{
    [Key]
    public int SuscriptorID { get; set; }

    [Required, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
    public DateTime FechaSuscripcion { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(64)]
    public string TokenBaja { get; set; } = string.Empty;
}
