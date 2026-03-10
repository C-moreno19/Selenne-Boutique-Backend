using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("PasswordResetTokens")]
public class PasswordResetToken
{
    [Key]
    public int ResetTokenID { get; set; }

    public int UsuarioID { get; set; }

    [Required, MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool Used { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey("UsuarioID")]
    public Usuario Usuario { get; set; } = null!;
}
