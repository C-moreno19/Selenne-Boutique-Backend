using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Models.Entities;

namespace SelenneApi.Services;

public class NotificationService : INotificationService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public NotificationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task CreateAsync(int usuarioId, string titulo, string mensaje, string tipo = "info", string? referencia = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Notificaciones.Add(new Notificacion
        {
            UsuarioID = usuarioId, Titulo = titulo, Mensaje = mensaje,
            Tipo = tipo, Referencia = referencia
        });
        await context.SaveChangesAsync();
    }

    public async Task CreateBulkAsync(List<int> usuarioIds, string titulo, string mensaje, string tipo = "info")
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        foreach (var id in usuarioIds)
            context.Notificaciones.Add(new Notificacion { UsuarioID = id, Titulo = titulo, Mensaje = mensaje, Tipo = tipo });
        await context.SaveChangesAsync();
    }

    public async Task CreateForAllAsync(string titulo, string mensaje, string tipo = "info")
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userIds = await context.Usuarios.Where(u => u.Estado == "activo").Select(u => u.UsuarioID).ToListAsync();
        foreach (var id in userIds)
            context.Notificaciones.Add(new Notificacion { UsuarioID = id, Titulo = titulo, Mensaje = mensaje, Tipo = tipo });
        await context.SaveChangesAsync();
    }

    public async Task CreateForRoleAsync(string rolNombre, string titulo, string mensaje, string tipo = "info")
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userIds = await context.Usuarios
            .Where(u => u.Estado == "activo" && u.Rol != null && u.Rol.Nombre == rolNombre)
            .Select(u => u.UsuarioID).ToListAsync();
        foreach (var id in userIds)
            context.Notificaciones.Add(new Notificacion { UsuarioID = id, Titulo = titulo, Mensaje = mensaje, Tipo = tipo });
        await context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(int notificacionId, int usuarioId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var n = await context.Notificaciones.FirstOrDefaultAsync(n => n.NotificacionID == notificacionId && n.UsuarioID == usuarioId);
        if (n != null) { n.Leida = true; n.FechaLeida = DateTime.Now; await context.SaveChangesAsync(); }
    }

    public async Task MarkAllAsReadAsync(int usuarioId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notifs = await context.Notificaciones.Where(n => n.UsuarioID == usuarioId && !n.Leida).ToListAsync();
        foreach (var n in notifs) { n.Leida = true; n.FechaLeida = DateTime.Now; }
        await context.SaveChangesAsync();
    }
}
