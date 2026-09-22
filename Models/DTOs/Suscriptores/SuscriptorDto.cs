namespace SelenneApi.Models.DTOs.Suscriptores;

public class SuscriptorDto
{
    public int SuscriptorID { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaSuscripcion { get; set; }
}

public class SuscribirseDto
{
    public string Email { get; set; } = string.Empty;
}

public class EnviarCampanaDto
{
    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}
