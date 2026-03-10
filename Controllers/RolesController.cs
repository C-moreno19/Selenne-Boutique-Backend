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
            RoleID = r.RoleID, Nombre = r.Nombre, Descripcion = r.Descripcion, Estado = r.Estado,
            Permisos = r.RolePermissions.Select(rp => rp.Permission.Nombre).ToList()
        }).ToList()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RolDto>>> GetById(int id)
    {
        if (!PermissionHelper.HasPermission(User, "roles:ver")) return Forbid();
        var r = await _db.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.RoleID == id);
        if (r == null) return NotFound(ApiResponse<object>.Fail("Rol no encontrado"));
        return Ok(ApiResponse<RolDto>.Ok(new RolDto
        {
            RoleID = r.RoleID, Nombre = r.Nombre, Descripcion = r.Descripcion, Estado = r.Estado,
            Permisos = r.RolePermissions.Select(rp => rp.Permission.Nombre).ToList()
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RolDto>>> Create([FromBody] CreateRolRequestDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "roles:crear")) return Forbid();
        if (await _db.Roles.AnyAsync(r => r.Nombre == dto.Nombre))
            return BadRequest(ApiResponse<object>.Fail("El nombre del rol ya existe"));

        var rol = new Rol { Nombre = dto.Nombre, Descripcion = dto.Descripcion };
        _db.Roles.Add(rol);
        await _db.SaveChangesAsync();

        foreach (var permId in dto.PermisoIds)
        {
            if (await _db.Permissions.AnyAsync(p => p.PermissionID == permId))
                _db.RolePermissions.Add(new RolePermission { RoleID = rol.RoleID, PermissionID = permId });
        }
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = rol.RoleID },
            ApiResponse<RolDto>.Ok(new RolDto { RoleID = rol.RoleID, Nombre = rol.Nombre }, "Rol creado"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<RolDto>>> Update(int id, [FromBody] CreateRolRequestDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "roles:editar")) return Forbid();
        var rol = await _db.Roles.Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.RoleID == id);
        if (rol == null) return NotFound(ApiResponse<object>.Fail("Rol no encontrado"));

        rol.Nombre = dto.Nombre;
        rol.Descripcion = dto.Descripcion;

        _db.RolePermissions.RemoveRange(rol.RolePermissions);
        foreach (var permId in dto.PermisoIds)
        {
            if (await _db.Permissions.AnyAsync(p => p.PermissionID == permId))
                _db.RolePermissions.Add(new RolePermission { RoleID = rol.RoleID, PermissionID = permId });
        }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<RolDto>.Ok(new RolDto { RoleID = rol.RoleID, Nombre = rol.Nombre }, "Rol actualizado"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        if (!PermissionHelper.HasPermission(User, "roles:eliminar")) return Forbid();
        var rol = await _db.Roles.FindAsync(id);
        if (rol == null) return NotFound(ApiResponse<object>.Fail("Rol no encontrado"));
        _db.Roles.Remove(rol);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { }, "Rol eliminado"));
    }

    [HttpGet("permisos")]
    public async Task<ActionResult<ApiResponse<object>>> GetPermisos()
    {
        if (!PermissionHelper.HasPermission(User, "roles:permisos")) return Forbid();
        var permisos = await _db.Permissions.Where(p => p.Estado == "activo")
            .Select(p => new { p.PermissionID, p.Nombre, p.Descripcion }).ToListAsync();
        return Ok(ApiResponse<object>.Ok(permisos));
    }
}
