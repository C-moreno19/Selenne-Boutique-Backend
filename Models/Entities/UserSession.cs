using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("UserSessions")]
public class UserSession
{
    [Key]
    public int SessionID { get; set; }

    public int UsuarioID { get; set; }
    public int? RefreshTokenID { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(50)]
    public string? IPAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastSeenAt { get; set; } = DateTime.Now;
    public bool Revoked { get; set; } = false;

    [ForeignKey("UsuarioID")]
    public Usuario Usuario { get; set; } = null!;

    [ForeignKey("RefreshTokenID")]
    public RefreshToken? RefreshToken { get; set; }
}
