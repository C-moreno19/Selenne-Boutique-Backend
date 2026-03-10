using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("RefreshTokens")]
public class RefreshToken
{
    [Key]
    public int RefreshTokenID { get; set; }

    public int UsuarioID { get; set; }

    [Required, MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool Revoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? RevokedAt { get; set; }

    [MaxLength(50)]
    public string? IPAddress { get; set; }

    [ForeignKey("UsuarioID")]
    public Usuario Usuario { get; set; } = null!;
}
