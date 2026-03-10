using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Models.DTOs.Carrito;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.Entities;
using SelenneApi.Helpers;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/carrito")]
[Authorize]
public class CarritoController : ControllerBase
{
    private readonly AppDbContext _db;
    public CarritoController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CarritoItemDto>>>> GetCarrito()
    {
        var userId = User.GetUserId();
        var items = await _db.Carrito
            .Include(c => c.Producto)
            .Where(c => c.UsuarioID == userId)
            .ToListAsync();

        var dto = items.Select(c => new CarritoItemDto
        {
            CarritoID = c.CarritoID,
            ProductoID = c.ProductoID,
            ProductoNombre = c.Producto.Nombre,
            ImagenProducto = c.Producto.ImagenPrincipal,
            PrecioUnitario = c.Producto.PrecioOferta ?? c.Producto.PrecioVenta,
            Cantidad = c.Cantidad,
            TallaSeleccionada = c.TallaSeleccionada,
            ColorSeleccionado = c.ColorSeleccionado,
            Subtotal = (c.Producto.PrecioOferta ?? c.Producto.PrecioVenta) * c.Cantidad
        }).ToList();

        return Ok(ApiResponse<List<CarritoItemDto>>.Ok(dto));
    }

    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<CarritoItemDto>>> AddItem([FromBody] AgregarCarritoDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "tienda:carrito"))
            return Forbid();

        var userId = User.GetUserId();
        var producto = await _db.Productos.FindAsync(dto.ProductoID);
        if (producto == null) return NotFound(ApiResponse<object>.Fail("Producto no encontrado"));
        if (producto.Estado != "activo") return BadRequest(ApiResponse<object>.Fail("Producto no disponible"));

        var existing = await _db.Carrito.FirstOrDefaultAsync(c =>
            c.UsuarioID == userId && c.ProductoID == dto.ProductoID &&
            c.TallaSeleccionada == dto.TallaSeleccionada && c.ColorSeleccionado == dto.ColorSeleccionado);

        if (existing != null)
        {
            existing.Cantidad += dto.Cantidad;
            await _db.SaveChangesAsync();
        }
        else
        {
            var item = new Carrito
            {
                UsuarioID = userId,
                ProductoID = dto.ProductoID,
                Cantidad = dto.Cantidad,
                TallaSeleccionada = dto.TallaSeleccionada,
                ColorSeleccionado = dto.ColorSeleccionado,
                FechaAgregado = DateTime.Now
            };
            _db.Carrito.Add(item);
            await _db.SaveChangesAsync();
        }

        return Ok(ApiResponse<object>.Ok(new { }, "Producto agregado al carrito"));
    }

    [HttpPut("items/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateItem(int id, [FromBody] ActualizarCantidadDto dto)
    {
        var userId = User.GetUserId();
        var item = await _db.Carrito.FirstOrDefaultAsync(c => c.CarritoID == id && c.UsuarioID == userId);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Item no encontrado"));

        if (dto.Cantidad <= 0)
        {
            _db.Carrito.Remove(item);
        }
        else
        {
            item.Cantidad = dto.Cantidad;
        }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { }, "Carrito actualizado"));
    }

    [HttpDelete("items/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveItem(int id)
    {
        var userId = User.GetUserId();
        var item = await _db.Carrito.FirstOrDefaultAsync(c => c.CarritoID == id && c.UsuarioID == userId);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Item no encontrado"));

        _db.Carrito.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { }, "Item eliminado"));
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<object>>> ClearCarrito()
    {
        var userId = User.GetUserId();
        var items = _db.Carrito.Where(c => c.UsuarioID == userId);
        _db.Carrito.RemoveRange(items);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { }, "Carrito vaciado"));
    }
}
