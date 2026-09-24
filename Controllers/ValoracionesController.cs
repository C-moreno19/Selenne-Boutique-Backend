using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Helpers;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.DTOs.Valoraciones;
using SelenneApi.Models.Entities;
using SelenneApi.Services;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/valoraciones")]
public class ValoracionesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notif;

    // Mismos estados que PedidosController considera una venta confirmada
    // (los que descuentan stock) — una reseña solo cuenta como "compra
    // verificada" si el pedido llegó a ese punto, no si quedó pendiente/rechazado.
    private static readonly string[] EstadosCompraVerificada =
        { "Aprobado", "Aprobada", "En proceso", "Enviado", "Entregado", "Completado", "Completada" };

    public ValoracionesController(AppDbContext db, INotificationService notif)
    {
        _db = db; _notif = notif;
    }

    [HttpGet("producto/{productoId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ValoracionDto>>>> GetByProducto(int productoId)
    {
        var valoraciones = await _db.Valoraciones
            .Include(v => v.Usuario)
            .Where(v => v.ProductoID == productoId && v.Estado == "aprobada")
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();

        return Ok(ApiResponse<List<ValoracionDto>>.Ok(valoraciones.Select(MapDto).ToList()));
    }

    [HttpPost, Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CrearValoracionDto dto)
    {
        var userId = User.GetUserId();
        if (dto.Puntuacion < 1 || dto.Puntuacion > 5)
            return BadRequest(ApiResponse<object>.Fail("La puntuación debe estar entre 1 y 5"));

        var producto = await _db.Productos.FindAsync(dto.ProductoID);
        if (producto == null) return NotFound(ApiResponse<object>.Fail("Producto no encontrado"));

        // Cualquier usuario logueado puede reseñar — no hace falta haber comprado.
        // Se sigue verificando la compra solo para mostrar el sello "Compra verificada".
        var compro = await _db.PedidoDetalles
            .Include(d => d.Pedido)
            .AnyAsync(d => d.ProductoID == dto.ProductoID
                && d.Pedido!.ClienteID == userId
                && EstadosCompraVerificada.Contains(d.Pedido.Estado));

        var yaReseño = await _db.Valoraciones.AnyAsync(v => v.ProductoID == dto.ProductoID && v.UsuarioID == userId);
        if (yaReseño)
            return BadRequest(ApiResponse<object>.Fail("Ya reseñaste este producto"));

        var valoracion = new Valoracion
        {
            ProductoID = dto.ProductoID,
            UsuarioID = userId,
            Puntuacion = dto.Puntuacion,
            Comentario = dto.Comentario,
            VerificadoCompra = compro,
            Estado = "pendiente",
            FechaCreacion = DateTime.UtcNow
        };
        _db.Valoraciones.Add(valoracion);
        await _db.SaveChangesAsync();

        var adminIds = await _db.Usuarios
            .Where(u => u.Estado == "activo" && (
                (u.Rol != null && u.Rol.Nombre == "Administrador") ||
                _db.RolePermissions.Any(rp => rp.RoleID == u.RoleID && rp.Permission.Nombre == "productos:editar")))
            .Select(u => u.UsuarioID)
            .Distinct()
            .ToListAsync();
        if (adminIds.Any())
            _ = _notif.CreateBulkAsync(adminIds, "Nueva reseña pendiente",
                $"\"{producto.Nombre}\" tiene una reseña esperando aprobación.", "info");

        return Ok(ApiResponse<object>.Ok(new { valoracion.ValoracionID }, "Reseña enviada. Se publicará cuando un administrador la apruebe."));
    }

    private static ValoracionDto MapDto(Valoracion v) => new()
    {
        ValoracionID = v.ValoracionID,
        ProductoID = v.ProductoID,
        UsuarioID = v.UsuarioID,
        NombreUsuario = v.Usuario != null ? v.Usuario.NombreCompleto : "Cliente",
        Puntuacion = v.Puntuacion,
        Comentario = v.Comentario,
        VerificadoCompra = v.VerificadoCompra,
        FechaCreacion = v.FechaCreacion,
        Estado = v.Estado
    };
}

[ApiController]
[Route("api/admin/valoraciones")]
[Authorize]
public class AdminValoracionesController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminValoracionesController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ValoracionDto>>>> GetAll([FromQuery] string? estado)
    {
        if (!PermissionHelper.HasPermission(User, "productos:editar")) return Forbid();

        var query = _db.Valoraciones.Include(v => v.Usuario).Include(v => v.Producto).AsQueryable();
        if (!string.IsNullOrEmpty(estado)) query = query.Where(v => v.Estado == estado);

        var valoraciones = await query
            .OrderByDescending(v => v.FechaCreacion)
            .Select(v => new ValoracionDto
            {
                ValoracionID = v.ValoracionID,
                ProductoID = v.ProductoID,
                ProductoNombre = v.Producto != null ? v.Producto.Nombre : null,
                UsuarioID = v.UsuarioID,
                NombreUsuario = v.Usuario != null ? v.Usuario.NombreCompleto : "Cliente",
                Puntuacion = v.Puntuacion,
                Comentario = v.Comentario,
                VerificadoCompra = v.VerificadoCompra,
                FechaCreacion = v.FechaCreacion,
                Estado = v.Estado
            })
            .ToListAsync();

        return Ok(ApiResponse<List<ValoracionDto>>.Ok(valoraciones));
    }

    [HttpPut("{id}/estado")]
    public async Task<ActionResult<ApiResponse<object>>> Moderar(int id, [FromBody] ModerarValoracionDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "productos:editar")) return Forbid();

        // La columna Estado tiene un CHECK constraint en la base de datos que
        // solo acepta 'pendiente' | 'aprobada' | 'rechazada' (concuerda en
        // genero con "reseña"/"valoración", no con "aprobado" en masculino).
        var estadosValidos = new[] { "aprobada", "rechazada" };
        if (!estadosValidos.Contains(dto.NuevoEstado))
            return BadRequest(ApiResponse<object>.Fail("Estado inválido"));

        var valoracion = await _db.Valoraciones.FindAsync(id);
        if (valoracion == null) return NotFound(ApiResponse<object>.Fail("Reseña no encontrada"));

        valoracion.Estado = dto.NuevoEstado;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { }, "Reseña actualizada"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Eliminar(int id)
    {
        if (!PermissionHelper.HasPermission(User, "productos:editar")) return Forbid();

        var valoracion = await _db.Valoraciones.FindAsync(id);
        if (valoracion == null) return NotFound(ApiResponse<object>.Fail("Reseña no encontrada"));

        _db.Valoraciones.Remove(valoracion);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { }, "Reseña eliminada"));
    }
}
