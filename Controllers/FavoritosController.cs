using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Helpers;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.Entities;

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
    public async Task<ActionResult<ApiResponse<object>>> Agregar(int productoId)
    {
        var userId = User.GetUserId();
        var existe = await _db.Favoritos.AnyAsync(f => f.UsuarioID == userId && f.ProductoID == productoId);
        if (!existe)
        {
            _db.Favoritos.Add(new Favorito { UsuarioID = userId, ProductoID = productoId });
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<object>.Ok(new { }, "Agregado a favoritos"));
    }

    [HttpDelete("{productoId}")]
    public async Task<ActionResult<ApiResponse<object>>> Eliminar(int productoId)
    {
        var userId = User.GetUserId();
        var fav = await _db.Favoritos.FirstOrDefaultAsync(f => f.UsuarioID == userId && f.ProductoID == productoId);
        if (fav != null)
        {
            _db.Favoritos.Remove(fav);
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<object>.Ok(new { }, "Eliminado de favoritos"));
    }
}
