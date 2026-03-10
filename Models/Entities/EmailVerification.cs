using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("EmailVerifications")]
public class EmailVerification
{
    [Key]
    public int VerificationID { get; set; }

    public int UsuarioID { get; set; }

    [Required, MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool Verified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey("UsuarioID")]
    public Usuario Usuario { get; set; } = null!;
}
