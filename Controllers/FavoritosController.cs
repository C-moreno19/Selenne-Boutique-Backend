using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.Entities;
using SelenneApi.Helpers;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/favoritos")]
[Authorize]
public class FavoritosController : ControllerBase
{
    private readonly AppDbContext _db;
    public FavoritosController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<int>>>> GetFavoritos()
    {
        var userId = User.GetUserId();
        var ids = await _db.Favoritos
            .Where(f => f.UsuarioID == userId)
            .Select(f => f.ProductoID)
            .ToListAsync();

        return Ok(ApiResponse<List<int>>.Ok(ids));
    }

    [HttpPost("{productoId}")]
    public async Task<ActionResult<ApiResponse<object>>> AddFavorito(int productoId)
    {
        var userId = User.GetUserId();

        var exists = await _db.Favoritos.AnyAsync(f => f.UsuarioID == userId && f.ProductoID == productoId);
        if (exists) return Ok(ApiResponse<object>.Ok(new { }, "Ya en favoritos"));

        var producto = await _db.Productos.FindAsync(productoId);
        if (producto == null) return NotFound(ApiResponse<object>.Fail("Producto no encontrado"));

        _db.Favoritos.Add(new Favorito
        {
            UsuarioID = userId,
            ProductoID = productoId,
            FechaAgregado = DateTime.Now
        });
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { }, "Agregado a favoritos"));
    }

    [HttpDelete("{productoId}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveFavorito(int productoId)
    {
        var userId = User.GetUserId();

        var favorito = await _db.Favoritos.FirstOrDefaultAsync(f => f.UsuarioID == userId && f.ProductoID == productoId);
        if (favorito == null) return NotFound(ApiResponse<object>.Fail("No encontrado en favoritos"));

        _db.Favoritos.Remove(favorito);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { }, "Eliminado de favoritos"));
    }
}
