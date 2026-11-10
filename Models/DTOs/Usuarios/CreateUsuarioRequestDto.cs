using System.ComponentModel.DataAnnotations;

namespace SelenneApi.Models.DTOs.Usuarios;

public class CreateUsuarioRequestDto
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [MinLength(9, ErrorMessage = "La contraseña debe tener más de 8 caracteres."), MaxLength(20, ErrorMessage = "La contraseña no puede superar los 20 caracteres."),
     RegularExpression(@"^(?=(.*\d){2})(?=.*[^a-zA-Z0-9\s]).{9,20}$",
         ErrorMessage = "La contraseña debe tener más de 8 caracteres, máximo 20, al menos 2 números y al menos 1 carácter especial.")]
    public string Contrasena { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Documento { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Estado { get; set; }
    public int? RoleID { get; set; }
}