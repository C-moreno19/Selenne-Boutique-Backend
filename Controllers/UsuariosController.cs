using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Helpers;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.DTOs.Usuarios;
using SelenneApi.Models.Entities;
using SelenneApi.Services;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly INotificationService _notif;
    private readonly IPermissionService _perms;

    public UsuariosController(AppDbContext db, IEmailService email, INotificationService notif, IPermissionService perms)
    {
        _db = db;
        _email = email;
        _notif = notif;
        _perms = perms;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var uid = User.GetUserId();
        if (!await _perms.HasPermissionAsync(uid, "usuarios:ver")) return Forbid();
        var list = await _db.Usuarios.Include(u => u.Rol).Select(u => new UsuarioDto
        {
            UsuarioID = u.UsuarioID,
            NombreCompleto = u.NombreCompleto,
            Email = u.Email,
            Telefono = u.Telefono,
            RoleID = u.RoleID,
            RolNombre = u.Rol != null ? u.Rol.Nombre : null,
            Estado = u.Estado,
            EmailVerificado = u.EmailVerificado,
            FechaRegistro = u.FechaRegistro,
            Ciudad = u.Ciudad
        }).ToListAsync();
        return Ok(ApiResponse<List<UsuarioDto>>.Ok(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var uid = User.GetUserId();
        if (uid != id && !await _perms.HasPermissionAsync(uid, "usuarios:ver")) return Forbid();
        var u = await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.UsuarioID == id);
        if (u == null) return NotFound(ApiResponse<object>.Fail("Usuario no encontrado"));
        return Ok(ApiResponse<UsuarioDto>.Ok(new UsuarioDto
        {
            UsuarioID = u.UsuarioID,
            NombreCompleto = u.NombreCompleto,
            Email = u.Email,
            Telefono = u.Telefono,
            Documento = u.Documento,
            Direccion = u.Direccion,
            Ciudad = u.Ciudad,
            RoleID = u.RoleID,
            RolNombre = u.Rol?.Nombre,
            Estado = u.Estado,
            EmailVerificado = u.EmailVerificado,
            FechaRegistro = u.FechaRegistro,
            FechaUltimoLogin = u.FechaUltimoLogin
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioRequestDto dto)
    {
        var uid = User.GetUserId();
        if (!await _perms.HasPermissionAsync(uid, "usuarios:crear")) return Forbid();
        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(ApiResponse<object>.Fail("El email ya existe"));
        var usuario = new Usuario
        {
            NombreCompleto = dto.NombreCompleto,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena),
            Telefono = dto.Telefono,
            Documento = dto.Documento,
            Direccion = dto.Direccion,
            Ciudad = dto.Ciudad,
            RoleID = dto.RoleID,
            Estado = dto.Estado ?? "activo"
        };
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        _ = _email.SendNewUserCreatedEmailAsync(usuario.Email, usuario.NombreCompleto, dto.Contrasena);
        return CreatedAtAction(nameof(GetById), new { id = usuario.UsuarioID }, ApiResponse<object>.Ok(new { usuario.UsuarioID }, "Usuario creado"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioRequestDto dto)
    {
        var uid = User.GetUserId();
        if (uid != id && !await _perms.HasPermissionAsync(uid, "usuarios:editar")) return Forbid();
        var u = await _db.Usuarios.FindAsync(id);
        if (u == null) return NotFound(ApiResponse<object>.Fail("No encontrado"));

        if (dto.NombreCompleto != null) u.NombreCompleto = dto.NombreCompleto;
        if (dto.Telefono != null) u.Telefono = dto.Telefono;
        if (dto.Documento != null) u.Documento = dto.Documento;
        if (dto.Direccion != null) u.Direccion = dto.Direccion;
        if (dto.Ciudad != null) u.Ciudad = dto.Ciudad;
        if (dto.RoleID.HasValue) u.RoleID = dto.RoleID;
        if (dto.Estado != null) u.Estado = dto.Estado;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { }, "Actualizado"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var uid = User.GetUserId();
        if (!await _perms.HasPermissionAsync(uid, "usuarios:eliminar")) return Forbid();
        var u = await _db.Usuarios.FindAsync(id);
        if (u == null) return NotFound(ApiResponse<object>.Fail("No encontrado"));

        var tokens = _db.RefreshTokens.Where(t => t.UsuarioID == id);
        _db.RefreshTokens.RemoveRange(tokens);
        var notifs = _db.Notificaciones.Where(n => n.UsuarioID == id);
        _db.Notificaciones.RemoveRange(notifs);

        _db.Usuarios.Remove(u);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { }, "Usuario eliminado permanentemente"));
    }

    [HttpPost("{id}/bloquear")]
    public async Task<IActionResult> Bloquear(int id)
    {
        var uid = User.GetUserId();
        if (!await _perms.HasPermissionAsync(uid, "usuarios:bloquear")) return Forbid();
        var u = await _db.Usuarios.FindAsync(id);
        if (u == null) return NotFound(ApiResponse<object>.Fail("No encontrado"));
        u.Estado = u.Estado == "activo" ? "inactivo" : "activo";
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { u.Estado }));
    }
}