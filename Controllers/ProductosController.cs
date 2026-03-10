using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Helpers;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.DTOs.Productos;
using SelenneApi.Models.Entities;
using SelenneApi.Services;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPermissionService _perms;
    public ProductosController(AppDbContext db, IPermissionService perms) { _db = db; _perms = perms; }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? estado, [FromQuery] int? categoriaId, [FromQuery] string? buscar)
    {
        var query = _db.Productos.Include(p => p.Categoria).Include(p => p.Marca).Include(p => p.TipoProducto)
            .Include(p => p.Imagenes).Include(p => p.StockVariantes).Include(p => p.ProductoTallas).ThenInclude(pt => pt.Talla)
            .Include(p => p.ProductoColores).ThenInclude(pc => pc.Color)
            .Include(p => p.ProductoMateriales).ThenInclude(pm => pm.Material)
            .Include(p => p.Valoraciones).AsQueryable();
        query = query.Where(p => p.Estado == (estado ?? "activo"));
        if (categoriaId.HasValue) query = query.Where(p => p.CategoriaPrincipalID == categoriaId);
        if (!string.IsNullOrEmpty(buscar)) query = query.Where(p => p.Nombre.Contains(buscar) || p.Codigo.Contains(buscar));
        var prods = await query.ToListAsync();
        return Ok(ApiResponse<List<ProductoDto>>.Ok(prods.Select(MapDto).ToList()));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Productos.Include(x => x.Categoria).Include(x => x.Marca).Include(x => x.TipoProducto)
            .Include(x => x.Imagenes).Include(x => x.StockVariantes).Include(x => x.ProductoTallas).ThenInclude(pt => pt.Talla)
            .Include(x => x.ProductoColores).ThenInclude(pc => pc.Color)
            .Include(x => x.ProductoMateriales).ThenInclude(pm => pm.Material)
            .Include(x => x.Valoraciones)
            .FirstOrDefaultAsync(x => x.ProductoID == id);
        if (p == null) return NotFound(ApiResponse<object>.Fail("Producto no encontrado"));
        return Ok(ApiResponse<ProductoDto>.Ok(MapDto(p)));
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Create([FromBody] CreateProductoRequestDto dto)
    {
        var uid = User.GetUserId();
        if (!await _perms.HasPermissionAsync(uid, "productos:crear")) return Forbid();
        if (await _db.Productos.AnyAsync(p => p.Codigo.ToLower() == dto.Codigo.ToLower()))
            return BadRequest(ApiResponse<object>.Fail("El código ya existe. Usa un código diferente."));
        var prod = new Producto
        {
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            DescripcionCorta = dto.DescripcionCorta,
            CategoriaPrincipalID = dto.CategoriaPrincipalID,
            TipoProductoID = dto.TipoProductoID,
            MarcaID = dto.MarcaID,
            PrecioCompra = dto.PrecioCompra,
            PrecioVenta = dto.PrecioVenta,
            PrecioOferta = dto.PrecioOferta,
            Stock = dto.Stock,
            ImagenPrincipal = dto.ImagenPrincipal
        };
        _db.Productos.Add(prod);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = prod.ProductoID }, ApiResponse<object>.Ok(new { prod.ProductoID }, "Producto creado"));
    }

    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductoRequestDto dto)
    {
        var uid = User.GetUserId();
        if (!await _perms.HasPermissionAsync(uid, "productos:editar")) return Forbid();
        var p = await _db.Productos.FindAsync(id);
        if (p == null) return NotFound(ApiResponse<object>.Fail("No encontrado"));
        if (dto.Nombre != null) p.Nombre = dto.Nombre;
        if (dto.Descripcion != null) p.Descripcion = dto.Descripcion;
        if (dto.CategoriaPrincipalID.HasValue) p.CategoriaPrincipalID = dto.CategoriaPrincipalID.Value;
        if (dto.TipoProductoID.HasValue) p.TipoProductoID = dto.TipoProductoID.Value;
        if (dto.MarcaID.HasValue) p.MarcaID = dto.MarcaID.Value;
        if (dto.PrecioVenta.HasValue) p.PrecioVenta = dto.PrecioVenta.Value;
        p.PrecioOferta = dto.PrecioOferta; // permite null para quitar descuento
        if (dto.Stock.HasValue) p.Stock = dto.Stock.Value;
        if (dto.Estado != null) p.Estado = dto.Estado;
        if (dto.ImagenPrincipal != null) p.ImagenPrincipal = dto.ImagenPrincipal;
        p.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null, "Actualizado"));
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var uid = User.GetUserId();
        if (!await _perms.HasPermissionAsync(uid, "productos:eliminar")) return Forbid();
        var p = await _db.Productos.FindAsync(id);
        if (p == null) return NotFound(ApiResponse<object>.Fail("No encontrado"));
        _db.Productos.Remove(p);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null, "Eliminado"));
    }


    [HttpPost("{id}/tallas"), Authorize]
    public async Task<IActionResult> SetTallas(int id, [FromBody] SetTallasDto dto)
    {
        var uid = User.GetUserId();
        if (!await _perms.HasPermissionAsync(uid, "productos:editar")) return Forbid();
        var p = await _db.Productos.Include(x => x.ProductoTallas).FirstOrDefaultAsync(x => x.ProductoID == id);
        if (p == null) return NotFound(ApiResponse<object>.Fail("No encontrado"));
        // Remove existing
        _db.Set<ProductoTalla>().RemoveRange(p.ProductoTallas);
        // Add new
        foreach (var t in dto.Tallas)
        {
            _db.Set<ProductoTalla>().Add(new ProductoTalla { ProductoID = id, TallaID = t.TallaID, StockTalla = t.Stock });
        }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null, "Tallas actualizadas"));
    }

    [HttpPost("{id}/colores"), Authorize]
    public async Task<IActionResult> SetColores(int id, [FromBody] SetColoresDto dto)
    {
        var uid = User.GetUserId();
        if (!await _perms.HasPermissionAsync(uid, "productos:editar")) return Forbid();
        var p = await _db.Productos.Include(x => x.ProductoColores).FirstOrDefaultAsync(x => x.ProductoID == id);
        if (p == null) return NotFound(ApiResponse<object>.Fail("No encontrado"));
        _db.Set<ProductoColor>().RemoveRange(p.ProductoColores);
        foreach (var colorId in dto.ColorIDs)
        {
            _db.Set<ProductoColor>().Add(new ProductoColor { ProductoID = id, ColorID = colorId });
        }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null, "Colores actualizados"));
    }



    [HttpPost("{id}/materiales"), Authorize]
    public async Task<IActionResult> SetMateriales(int id, [FromBody] SetMaterialesDto dto)
    {
        var uid = User.GetUserId();
        if (!await _perms.HasPermissionAsync(uid, "productos:editar")) return Forbid();
        var p = await _db.Productos.Include(x => x.ProductoMateriales).FirstOrDefaultAsync(x => x.ProductoID == id);
        if (p == null) return NotFound(ApiResponse<object>.Fail("No encontrado"));
        _db.Set<ProductoMaterial>().RemoveRange(p.ProductoMateriales);
        foreach (var matId in dto.MaterialIDs)
        {
            _db.Set<ProductoMaterial>().Add(new ProductoMaterial { ProductoID = id, MaterialID = matId });
        }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null, "Materiales actualizados"));
    }

    // POST /api/productos/{id}/variantes — Stock por talla+color
    [HttpPost("{id}/variantes")]
    public async Task<IActionResult> SetVariantes(int id, [FromBody] SetVariantesDto dto)
    {
        var p = await _db.Productos
            .Include(x => x.StockVariantes)
            .FirstOrDefaultAsync(x => x.ProductoID == id);
        if (p == null) return NotFound(ApiResponse<object>.Fail("Producto no encontrado"));

        _db.Set<ProductoStockVariante>().RemoveRange(p.StockVariantes ?? new List<ProductoStockVariante>());

        foreach (var v in dto.Variantes)
        {
            _db.Set<ProductoStockVariante>().Add(new ProductoStockVariante
            {
                ProductoID = id,
                TallaNombre = v.TallaNombre,
                ColorNombre = v.ColorNombre,
                Stock = v.Stock
            });
        }

        // Actualizar stock general como suma de variantes (o mantener si no hay variantes)
        if (dto.Variantes.Any())
            p.Stock = dto.Variantes.Sum(v => v.Stock);

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null, "Variantes actualizadas"));
    }

    [HttpPost("{id}/imagenes"), Authorize]
    public async Task<IActionResult> SetImagenes(int id, [FromBody] SetImagenesDto dto)
    {
        var uid = User.GetUserId();
        if (!await _perms.HasPermissionAsync(uid, "productos:editar")) return Forbid();
        var p = await _db.Productos.Include(x => x.Imagenes).FirstOrDefaultAsync(x => x.ProductoID == id);
        if (p == null) return NotFound(ApiResponse<object>.Fail("No encontrado"));
        _db.Set<ProductoImagen>().RemoveRange(p.Imagenes);
        int orden = 0;
        foreach (var img in dto.Imagenes.Where(i => !string.IsNullOrWhiteSpace(i.URL)))
        {
            _db.Set<ProductoImagen>().Add(new ProductoImagen
            {
                ProductoID = id,
                URL = img.URL,
                Orden = orden++,
                ColorNombre = string.IsNullOrWhiteSpace(img.ColorNombre) ? null : img.ColorNombre
            });
        }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null, "Imágenes actualizadas"));
    }

    [HttpPost("{id}/descuento"), Authorize]
    public async Task<IActionResult> AplicarDescuento(int id, [FromBody] AplicarDescuentoDto dto)
    {
        var uid = User.GetUserId();
        if (!await _perms.HasPermissionAsync(uid, "productos:descuento")) return Forbid();
        var p = await _db.Productos.FindAsync(id);
        if (p == null) return NotFound(ApiResponse<object>.Fail("No encontrado"));
        p.PrecioOferta = Math.Round(p.PrecioVenta * (1 - dto.PorcentajeDescuento / 100), 2);
        p.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { p.PrecioOferta }, "Descuento aplicado"));
    }

    private static ProductoDto MapDto(Producto p) => new()
    {
        ProductoID = p.ProductoID,
        Codigo = p.Codigo,
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        DescripcionCorta = p.DescripcionCorta,
        CategoriaPrincipalID = p.CategoriaPrincipalID,
        CategoriaNombre = p.Categoria?.Nombre,
        TipoProductoID = p.TipoProductoID,
        TipoNombre = p.TipoProducto?.Nombre,
        MarcaID = p.MarcaID,
        MarcaNombre = p.Marca?.Nombre,
        PrecioCompra = p.PrecioCompra,
        PrecioVenta = p.PrecioVenta,
        PrecioOferta = p.PrecioOferta,
        Stock = p.Stock,
        ImagenPrincipal = p.ImagenPrincipal,
        Estado = p.Estado,
        FechaCreacion = p.FechaCreacion,
        Imagenes = p.Imagenes?.Select(i => new ImagenDto { URL = i.URL, ColorNombre = i.ColorNombre }).ToList() ?? new(),
        Variantes = p.StockVariantes?.Select(v => new VarianteStockDto
        {
            TallaNombre = v.TallaNombre,
            ColorNombre = v.ColorNombre,
            Stock = v.Stock
        }).ToList() ?? new(),
        AgotadoGeneral = (p.StockVariantes == null || !p.StockVariantes.Any())
            ? p.Stock <= 0
            : p.StockVariantes.Sum(v => v.Stock) <= 0,
        Tallas = p.ProductoTallas?.Select(pt => new TallaStockDto { TallaID = pt.TallaID, Nombre = pt.Talla?.Nombre ?? "", Stock = pt.StockTalla }).ToList() ?? new(),
        Colores = p.ProductoColores?.Select(pc => new ColorDto { ColorID = pc.ColorID, Nombre = pc.Color?.Nombre ?? "", CodigoHex = pc.Color?.CodigoHex }).ToList() ?? new(),
        Materiales = p.ProductoMateriales?.Select(pm => pm.Material?.Nombre ?? "").Where(m => m != "").ToList() ?? new(),
        PromedioValoracion = p.Valoraciones?.Any() == true ? p.Valoraciones.Average(v => v.Puntuacion) : null,
        TotalValoraciones = p.Valoraciones?.Count ?? 0
    };
}