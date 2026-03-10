using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.DTOs.Notificaciones;
using SelenneApi.Services;
using SelenneApi.Helpers;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/notificaciones")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notif;
    public NotificacionesController(AppDbContext db, INotificationService notif)
    {
        _db = db; _notif = notif;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificacionDto>>>> GetAll()
    {
        var userId = User.GetUserId();
        var notifs = await _db.Notificaciones
            .Where(n => n.UsuarioID == userId)
            .OrderByDescending(n => n.FechaCreacion)
            .Select(n => new NotificacionDto
            {
                NotificacionID = n.NotificacionID,
                Titulo = n.Titulo, Mensaje = n.Mensaje, Tipo = n.Tipo,
                Leida = n.Leida, FechaCreacion = n.FechaCreacion, Referencia = n.Referencia
            }).ToListAsync();

        return Ok(ApiResponse<List<NotificacionDto>>.Ok(notifs));
    }

    [HttpPut("{id}/marcar-leida")]
    public async Task<ActionResult<ApiResponse<object>>> MarcarLeida(int id)
    {
        var userId = User.GetUserId();
        var n = await _db.Notificaciones.FirstOrDefaultAsync(n => n.NotificacionID == id && n.UsuarioID == userId);
        if (n == null) return NotFound(ApiResponse<object>.Fail("Notificacion no encontrada"));
        n.Leida = true;
        n.FechaLeida = DateTime.Now;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { }, "Notificacion marcada como leida"));
    }

    [HttpPost("marcar-todas-leidas")]
    public async Task<ActionResult<ApiResponse<object>>> MarcarTodasLeidas()
    {
        var userId = User.GetUserId();
        var notifs = await _db.Notificaciones
            .Where(n => n.UsuarioID == userId && !n.Leida).ToListAsync();
        foreach (var n in notifs) { n.Leida = true; n.FechaLeida = DateTime.Now; }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { count = notifs.Count }, "Todas marcadas como leidas"));
    }
}

[ApiController]
[Route("api/admin/notificaciones")]
[Authorize]
public class AdminNotificacionesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notif;
    public AdminNotificacionesController(AppDbContext db, INotificationService notif)
    {
        _db = db; _notif = notif;
    }

    [HttpPost("enviar")]
    public async Task<ActionResult<ApiResponse<object>>> Enviar([FromBody] EnviarNotificacionDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "notif:enviar"))
            return Forbid();

        List<int> userIds;
        if (dto.TodosLosUsuarios)
            userIds = await _db.Usuarios.Where(u => u.Estado == "activo").Select(u => u.UsuarioID).ToListAsync();
        else
            userIds = dto.UsuarioIds ?? new List<int>();

        if (!userIds.Any())
            return BadRequest(ApiResponse<object>.Fail("No hay destinatarios"));

        await _notif.CreateBulkAsync(userIds, dto.Titulo, dto.Mensaje, dto.Tipo);
        return Ok(ApiResponse<object>.Ok(new { count = userIds.Count }, "Notificaciones enviadas"));
    }
}
