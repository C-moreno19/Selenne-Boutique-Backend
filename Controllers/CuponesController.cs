using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Helpers;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.DTOs.Cupones;
using SelenneApi.Models.Entities;
using SelenneApi.Services;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/cupones")]
public class CuponesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICuponService _cupones;

    public CuponesController(AppDbContext db, ICuponService cupones)
    {
        _db = db; _cupones = cupones;
    }

    // POST /api/cupones/validar — publico, lo usa el checkout para mostrar el descuento
    [HttpPost("validar")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CuponValidoDto>>> Validar([FromBody] ValidarCuponDto dto)
    {
        var resultado = await _cupones.ValidarAsync(dto.Codigo, dto.Subtotal);
        if (!resultado.Valido || resultado.Cupon == null)
            return BadRequest(ApiResponse<object>.Fail(resultado.Error ?? "Cupón inválido"));

        return Ok(ApiResponse<CuponValidoDto>.Ok(new CuponValidoDto
        {
            CuponID = resultado.Cupon.CuponID,
            Codigo = resultado.Cupon.Codigo,
            TipoDescuento = resultado.Cupon.TipoDescuento,
            ValorDescuento = resultado.Cupon.ValorDescuento,
            MontoDescuento = resultado.MontoDescuento
        }));
    }

    // GET /api/cupones/publicos — publico, lo usa la tienda para anunciar
    // cupones vigentes (banner de la home) sin exponer datos internos de uso.
    [HttpGet("publicos")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<CuponPublicoDto>>>> GetPublicos()
    {
        var ahora = DateTime.UtcNow;
        var cupones = await _db.Cupones
            .Where(c => c.Activo
                && (c.FechaInicio == null || c.FechaInicio <= ahora)
                && (c.FechaExpiracion == null || c.FechaExpiracion >= ahora)
                && (c.UsosMaximos == null || c.UsosActuales < c.UsosMaximos))
            .OrderByDescending(c => c.FechaCreacion)
            .Select(c => new CuponPublicoDto
            {
                Codigo = c.Codigo,
                TipoDescuento = c.TipoDescuento,
                ValorDescuento = c.ValorDescuento,
                MontoMinimo = c.MontoMinimo,
                FechaExpiracion = c.FechaExpiracion
            })
            .ToListAsync();

        return Ok(ApiResponse<List<CuponPublicoDto>>.Ok(cupones));
    }

    [HttpGet, Authorize]
    public async Task<ActionResult<ApiResponse<List<CuponDto>>>> GetAll()
    {
        var uid = User.GetUserId();
        if (!PermissionHelper.HasPermission(User, "productos:editar")) return Forbid();

        var cupones = await _db.Cupones.OrderByDescending(c => c.FechaCreacion).ToListAsync();
        return Ok(ApiResponse<List<CuponDto>>.Ok(cupones.Select(MapDto).ToList()));
    }

    [HttpPost, Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CrearCuponDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "productos:editar")) return Forbid();

        var codigo = dto.Codigo.Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(codigo))
            return BadRequest(ApiResponse<object>.Fail("El código es obligatorio"));
        if (dto.TipoDescuento != "porcentaje" && dto.TipoDescuento != "monto")
            return BadRequest(ApiResponse<object>.Fail("Tipo de descuento inválido"));
        if (dto.ValorDescuento <= 0)
            return BadRequest(ApiResponse<object>.Fail("El valor del descuento debe ser mayor a 0"));
        if (dto.TipoDescuento == "porcentaje" && dto.ValorDescuento > 100)
            return BadRequest(ApiResponse<object>.Fail("El porcentaje no puede ser mayor a 100"));
        if (await _db.Cupones.AnyAsync(c => c.Codigo == codigo))
            return BadRequest(ApiResponse<object>.Fail("Ya existe un cupón con ese código"));

        var cupon = new Cupon
        {
            Codigo = codigo,
            TipoDescuento = dto.TipoDescuento,
            ValorDescuento = dto.ValorDescuento,
            MontoMinimo = dto.MontoMinimo,
            UsosMaximos = dto.UsosMaximos,
            FechaInicio = dto.FechaInicio,
            FechaExpiracion = dto.FechaExpiracion,
        };
        _db.Cupones.Add(cupon);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), ApiResponse<object>.Ok(new { cupon.CuponID }, "Cupón creado"));
    }

    [HttpPut("{id}"), Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] ActualizarCuponDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "productos:editar")) return Forbid();

        var cupon = await _db.Cupones.FindAsync(id);
        if (cupon == null) return NotFound(ApiResponse<object>.Fail("Cupón no encontrado"));

        if (dto.ValorDescuento.HasValue) cupon.ValorDescuento = dto.ValorDescuento.Value;
        cupon.MontoMinimo = dto.MontoMinimo ?? cupon.MontoMinimo;
        if (dto.UsosMaximos.HasValue) cupon.UsosMaximos = dto.UsosMaximos;
        if (dto.FechaInicio.HasValue) cupon.FechaInicio = dto.FechaInicio;
        if (dto.FechaExpiracion.HasValue) cupon.FechaExpiracion = dto.FechaExpiracion;
        if (dto.Activo.HasValue) cupon.Activo = dto.Activo.Value;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null, "Cupón actualizado"));
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        if (!PermissionHelper.HasPermission(User, "productos:editar")) return Forbid();

        var cupon = await _db.Cupones.FindAsync(id);
        if (cupon == null) return NotFound(ApiResponse<object>.Fail("Cupón no encontrado"));

        if (await _db.Pedidos.AnyAsync(p => p.CuponID == id))
        {
            cupon.Activo = false;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "El cupón tiene pedidos asociados; se desactivó en vez de eliminarse"));
        }

        _db.Cupones.Remove(cupon);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null, "Cupón eliminado"));
    }

    private static CuponDto MapDto(Cupon c) => new()
    {
        CuponID = c.CuponID,
        Codigo = c.Codigo,
        TipoDescuento = c.TipoDescuento,
        ValorDescuento = c.ValorDescuento,
        MontoMinimo = c.MontoMinimo,
        UsosMaximos = c.UsosMaximos,
        UsosActuales = c.UsosActuales,
        FechaInicio = c.FechaInicio,
        FechaExpiracion = c.FechaExpiracion,
        Activo = c.Activo,
        FechaCreacion = c.FechaCreacion
    };
}
