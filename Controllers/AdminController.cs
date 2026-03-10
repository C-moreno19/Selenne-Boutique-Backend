using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Models.DTOs;
using SelenneApi.Helpers;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) { _db = db; }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<object>>> Dashboard()
    {
        var totalUsuarios = await _db.Usuarios.CountAsync();
        var totalProductos = await _db.Productos.CountAsync();
        var totalPedidos = await _db.Pedidos.CountAsync();
        var pedidosPendientes = await _db.Pedidos.CountAsync(p => p.Estado == "Pendiente");
        var totalVentas = await _db.Pedidos
            .Where(p => p.Estado == "Completada" || p.Estado == "Entregado")
            .SumAsync(p => (decimal?)p.Total) ?? 0;

        var productosStockBajo = await _db.Productos
            .Where(p => p.Stock < 5 && p.Estado == "activo")
            .Select(p => new { p.ProductoID, p.Nombre, p.Stock })
            .Take(10).ToListAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            totalUsuarios,
            totalProductos,
            totalPedidos,
            pedidosPendientes,
            totalVentas,
            productosStockBajo
        }));
    }

    [HttpGet("auditoria")]
    public async Task<ActionResult<ApiResponse<object>>> GetAuditoria(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (!PermissionHelper.HasPermission(User, "config:auditoria")) return Forbid();
        var total = await _db.AuditoriaPerfil.CountAsync();
        var items = await _db.AuditoriaPerfil
            .Include(a => a.Usuario)
            .OrderByDescending(a => a.FechaCambio)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new
            {
                a.AuditoriaID, a.TipoCambio, a.CampoModificado,
                a.ValorAnterior, a.ValorNuevo, a.FechaCambio, a.Origen,
                usuarioNombre = a.Usuario.NombreCompleto
            }).ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { total, page, pageSize, data = items }));
    }
}
