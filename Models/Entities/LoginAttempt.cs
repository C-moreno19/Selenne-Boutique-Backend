using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenneApi.Models.Entities;

[Table("LoginAttempts")]
public class LoginAttempt
{
    [Key]
    public int AttemptID { get; set; }

    public int? UsuarioID { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    public DateTime AttemptAt { get; set; } = DateTime.Now;
    public bool Successful { get; set; } = false;

    [MaxLength(50)]
    public string? IPAddress { get; set; }

    [ForeignKey("UsuarioID")]
    public Usuario? Usuario { get; set; }
}
