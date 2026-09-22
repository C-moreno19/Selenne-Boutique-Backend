using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Helpers;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.DTOs.Suscriptores;
using SelenneApi.Models.Entities;
using SelenneApi.Services;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/suscriptores")]
public class SuscriptoresController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public SuscriptoresController(AppDbContext db, IConfiguration config)
    {
        _db = db; _config = config;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> Suscribirse([FromBody] SuscribirseDto dto)
    {
        var email = (dto.Email ?? "").Trim().ToLower();
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return BadRequest(ApiResponse<object>.Fail("Ingresa un correo válido"));

        var existente = await _db.Suscriptores.FirstOrDefaultAsync(s => s.Email == email);
        if (existente != null)
        {
            if (existente.Activo)
                return Ok(ApiResponse<object>.Ok(new { }, "Ya estabas suscrito a nuestro boletín"));
            existente.Activo = true;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { }, "¡Bienvenido de nuevo! Te suscribiste correctamente"));
        }

        var suscriptor = new Suscriptor
        {
            Email = email,
            TokenBaja = Guid.NewGuid().ToString("N"),
        };
        _db.Suscriptores.Add(suscriptor);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { }, "¡Gracias por suscribirte!"));
    }

    [HttpGet("baja/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> DarseDeBaja(string token)
    {
        string Pagina(string titulo, string mensaje) =>
            "<!DOCTYPE html><html lang='es'><head><meta charset='UTF-8'><meta name='viewport' content='width=device-width,initial-scale=1'><title>Selenne Boutique</title>" +
            "<style>body{font-family:Arial,Helvetica,sans-serif;background:#fdf2f8;display:flex;align-items:center;justify-content:center;min-height:100vh;margin:0;padding:24px}" +
            ".card{background:#fff;border-radius:16px;box-shadow:0 12px 40px rgba(214,83,145,.15);max-width:420px;width:100%;padding:36px;text-align:center}" +
            "h1{color:#d65391;font-size:20px;margin:0 0 12px}p{color:#6b7280;font-size:14px;line-height:1.6;margin:0}</style></head>" +
            "<body><div class='card'><h1>" + titulo + "</h1><p>" + mensaje + "</p></div></body></html>";

        var suscriptor = await _db.Suscriptores.FirstOrDefaultAsync(s => s.TokenBaja == token);
        if (suscriptor == null)
            return Content(Pagina("Enlace no válido", "Este enlace ya fue usado o no existe."), "text/html; charset=utf-8");

        suscriptor.Activo = false;
        await _db.SaveChangesAsync();
        return Content(Pagina("Listo", "Te diste de baja de nuestro boletín. No recibirás más correos promocionales de Selenne Boutique."), "text/html; charset=utf-8");
    }
}

[ApiController]
[Route("api/admin/suscriptores")]
[Authorize]
public class AdminSuscriptoresController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;

    public AdminSuscriptoresController(AppDbContext db, IEmailService email, IConfiguration config)
    {
        _db = db; _email = email; _config = config;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SuscriptorDto>>>> GetAll()
    {
        if (!PermissionHelper.HasPermission(User, "productos:editar")) return Forbid();

        var lista = await _db.Suscriptores
            .OrderByDescending(s => s.FechaSuscripcion)
            .Select(s => new SuscriptorDto
            {
                SuscriptorID = s.SuscriptorID,
                Email = s.Email,
                Activo = s.Activo,
                FechaSuscripcion = s.FechaSuscripcion,
            })
            .ToListAsync();

        return Ok(ApiResponse<List<SuscriptorDto>>.Ok(lista));
    }

    [HttpPost("enviar")]
    public async Task<ActionResult<ApiResponse<object>>> EnviarCampana([FromBody] EnviarCampanaDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "productos:editar")) return Forbid();

        if (string.IsNullOrWhiteSpace(dto.Asunto) || string.IsNullOrWhiteSpace(dto.Mensaje))
            return BadRequest(ApiResponse<object>.Fail("Asunto y mensaje son obligatorios"));

        var activos = await _db.Suscriptores.Where(s => s.Activo).ToListAsync();
        if (!activos.Any())
            return BadRequest(ApiResponse<object>.Fail("No hay suscriptores activos"));

        var baseUrl = _config["AppSettings:BaseUrl"] ?? "https://selenne-boutique-backend.onrender.com";
        foreach (var s in activos)
        {
            var unsubscribeUrl = $"{baseUrl}/api/suscriptores/baja/{s.TokenBaja}";
            _ = _email.SendCampaignEmailAsync(s.Email, dto.Asunto, dto.Mensaje, unsubscribeUrl);
        }

        return Ok(ApiResponse<object>.Ok(new { enviados = activos.Count }, $"Correo enviado a {activos.Count} suscriptores"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Eliminar(int id)
    {
        if (!PermissionHelper.HasPermission(User, "productos:editar")) return Forbid();

        var suscriptor = await _db.Suscriptores.FindAsync(id);
        if (suscriptor == null) return NotFound(ApiResponse<object>.Fail("Suscriptor no encontrado"));

        _db.Suscriptores.Remove(suscriptor);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { }, "Suscriptor eliminado"));
    }
}
