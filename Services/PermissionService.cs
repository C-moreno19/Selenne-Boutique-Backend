using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;

namespace SelenneApi.Services;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;
    public PermissionService(AppDbContext context) { _context = context; }

    public async Task<List<string>> GetUserPermissionsAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);
        if (usuario?.RoleID == null) return new List<string>();
        return await _context.RolePermissions
            .Where(rp => rp.RoleID == usuario.RoleID)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission.Nombre)
            .ToListAsync();
    }

    public async Task<bool> HasPermissionAsync(int usuarioId, string permiso)
    {
        // Administradores tienen acceso total
        var usuario = await _context.Usuarios.Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);
        if (usuario?.Rol?.Nombre == "Administrador") return true;

        var perms = await GetUserPermissionsAsync(usuarioId);
        return perms.Contains(permiso);
    }
}