using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.DTOs.Roles;
using SelenneApi.Models.Entities;
using SelenneApi.Helpers;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _db;
    public RolesController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RolDto>>>> GetAll()
    {
        if (!PermissionHelper.HasPermission(User, "roles:ver")) return Forbid();
        var roles = await _db.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .ToListAsync();
        return Ok(ApiResponse<List<RolDto>>.Ok(roles.Select(r => new RolDto
        {
            RoleID = r.RoleID,
            Nombre = r.Nombre,
            Descripcion = r.Descripcion,
            Estado = r.Estado,
            Permisos = r.RolePermissions.Select(rp => rp.Permission.Nombre).ToList()
        }).ToList()));
    }

    [HttpGet("permisos")]
    public async Task<ActionResult<ApiResponse<List<PermisoDto>>>> GetPermisos()
    {
        var permisos = await _db.Permissions.Where(p => p.Estado == "activo").ToListAsync();
        return Ok(ApiResponse<List<PermisoDto>>.Ok(permisos.Select(p => new PermisoDto
        {
            PermissionID = p.PermissionID,
            Nombre = p.Nombre
        }).ToList()));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<RolDto>>> Update(int id, [FromBody] UpdateRolDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "roles:permisos") &&
            !PermissionHelper.HasPermission(User, "roles:editar")) return Forbid();

        var rol = await _db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.RoleID == id);

        if (rol == null) return NotFound(ApiResponse<RolDto>.Fail("Rol no encontrado"));

        if (!string.IsNullOrWhiteSpace(dto.Nombre)) rol.Nombre = dto.Nombre;
        if (dto.Descripcion != null) rol.Descripcion = dto.Descripcion;

        if (dto.PermisoIds != null)
        {
            // Eliminar permisos actuales
            _db.RolePermissions.RemoveRange(rol.RolePermissions);

            // Agregar nuevos permisos
            foreach (var permId in dto.PermisoIds)
            {
                _db.RolePermissions.Add(new RolePermission
                {
                    RoleID = rol.RoleID,
                    PermissionID = permId
                });
            }
        }

        await _db.SaveChangesAsync();

        // Recargar para devolver datos actualizados
        await _db.Entry(rol).Collection(r => r.RolePermissions).Query()
            .Include(rp => rp.Permission).LoadAsync();

        return Ok(ApiResponse<RolDto>.Ok(new RolDto
        {
            RoleID = rol.RoleID,
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion,
            Estado = rol.Estado,
            Permisos = rol.RolePermissions.Select(rp => rp.Permission.Nombre).ToList()
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RolDto>>> Create([FromBody] CreateRolDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "roles:crear")) return Forbid();

        var rol = new Rol
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion ?? "",
            Estado = "activo"
        };
        _db.Roles.Add(rol);
        await _db.SaveChangesAsync();

        if (dto.PermisoIds != null)
        {
            foreach (var permId in dto.PermisoIds)
            {
                _db.RolePermissions.Add(new RolePermission
                {
                    RoleID = rol.RoleID,
                    PermissionID = permId
                });
            }
            await _db.SaveChangesAsync();
        }

        return Ok(ApiResponse<RolDto>.Ok(new RolDto
        {
            RoleID = rol.RoleID,
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion,
            Estado = rol.Estado,
            Permisos = new List<string>()
        }));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
    {
        if (!PermissionHelper.HasPermission(User, "roles:eliminar")) return Forbid();

        var rol = await _db.Roles.Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.RoleID == id);
        if (rol == null) return NotFound(ApiResponse<string>.Fail("Rol no encontrado"));

        _db.RolePermissions.RemoveRange(rol.RolePermissions);
        _db.Roles.Remove(rol);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("Rol eliminado"));
    }
}