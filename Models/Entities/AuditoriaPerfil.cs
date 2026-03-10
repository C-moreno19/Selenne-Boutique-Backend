using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("AuditoriaPerfil")]
public class AuditoriaPerfil
{
    [Key]
    public int AuditoriaID { get; set; }

    public int UsuarioID { get; set; }

    [Required, MaxLength(50)]
    public string TipoCambio { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CampoModificado { get; set; }

    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public DateTime FechaCambio { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? Origen { get; set; }

    [ForeignKey("UsuarioID")]
    public Usuario Usuario { get; set; } = null!;
}
