namespace SelenneApi.Models.DTOs.Valoraciones;

public class ValoracionDto
{
    public int ValoracionID { get; set; }
    public int ProductoID { get; set; }
    public string? ProductoNombre { get; set; }
    public int UsuarioID { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public bool VerificadoCompra { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class CrearValoracionDto
{
    public int ProductoID { get; set; }
    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
}

public class ModerarValoracionDto
{
    public string NuevoEstado { get; set; } = string.Empty;
}
