using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Models.DTOs;
using SelenneApi.Helpers;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReportesController(AppDbContext db) { _db = db; }

    [HttpGet("ventas")]
    public async Task<ActionResult<ApiResponse<object>>> ReporteVentas(
        [FromQuery] DateTime? desde = null, [FromQuery] DateTime? hasta = null)
    {
        if (!PermissionHelper.HasPermission(User, "reportes:ventas")) return Forbid();
        var d = desde ?? DateTime.Now.AddMonths(-1);
        var h = hasta ?? DateTime.Now;
        var pedidos = await _db.Pedidos.Where(p => p.FechaPedido >= d && p.FechaPedido <= h).ToListAsync();
        return Ok(ApiResponse<object>.Ok(new
        {
            totalPedidos = pedidos.Count,
            totalVentas = pedidos.Sum(p => p.Total),
            promedioPorPedido = pedidos.Any() ? pedidos.Average(p => p.Total) : 0
        }));
    }

    [HttpGet("inventario")]
    public async Task<ActionResult<ApiResponse<object>>> ReporteInventario()
    {
        if (!PermissionHelper.HasPermission(User, "reportes:inventario")) return Forbid();
        var productos = await _db.Productos.Include(p => p.Categoria).Where(p => p.Estado == "activo").ToListAsync();
        return Ok(ApiResponse<object>.Ok(new
        {
            totalProductos = productos.Count,
            stockTotal = productos.Sum(p => p.Stock),
            productosStockBajo = productos.Where(p => p.Stock < 5).Select(p => new { p.ProductoID, p.Nombre, p.Stock }).ToList()
        }));
    }

    [HttpGet("clientes")]
    public async Task<ActionResult<ApiResponse<object>>> ReporteClientes()
    {
        if (!PermissionHelper.HasPermission(User, "reportes:clientes")) return Forbid();
        var usuarios = await _db.Usuarios.Include(u => u.Rol).ToListAsync();
        return Ok(ApiResponse<object>.Ok(new
        {
            totalClientes = usuarios.Count,
            clientesActivos = usuarios.Count(u => u.Estado == "activo"),
            clientesBloqueados = usuarios.Count(u => u.Estado == "inactivo")
        }));
    }

    [HttpGet("financiero")]
    public async Task<ActionResult<ApiResponse<object>>> ReporteFinanciero([FromQuery] int anio = 0)
    {
        if (!PermissionHelper.HasPermission(User, "reportes:financiero")) return Forbid();
        if (anio == 0) anio = DateTime.Now.Year;
        var pedidos = await _db.Pedidos
            .Where(p => p.FechaPedido.Year == anio && (p.Estado == "Completada" || p.Estado == "Entregado"))
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(new
        {
            anio,
            totalAnual = pedidos.Sum(p => p.Total),
            totalPedidos = pedidos.Count
        }));
    }
}
