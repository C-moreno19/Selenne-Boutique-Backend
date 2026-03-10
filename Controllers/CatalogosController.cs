using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.Entities;

namespace SelenneApi.Controllers;

// ─────────────────────────────────────────────
// COLORES
// ─────────────────────────────────────────────
[ApiController]
[Route("api/colores")]
[Authorize]
public class ColoresController : ControllerBase
{
    private readonly AppDbContext _db;
    public ColoresController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Colores.OrderBy(c => c.Nombre).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Colores.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Color no encontrado"));
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ColorRequestDto dto)
    {
        if (await _db.Colores.AnyAsync(c => c.Nombre == dto.Nombre))
            return BadRequest(ApiResponse<object>.Fail("Ya existe un color con ese nombre"));
        var item = new Color { Nombre = dto.Nombre, CodigoHex = dto.CodigoHex, Estado = "activo" };
        _db.Colores.Add(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ColorRequestDto dto)
    {
        var item = await _db.Colores.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Color no encontrado"));
        item.Nombre = dto.Nombre;
        item.CodigoHex = dto.CodigoHex;
        if (dto.Estado != null) item.Estado = dto.Estado;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Colores.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Color no encontrado"));
        _db.Colores.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok("Color eliminado"));
    }
}

public class ColorRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? CodigoHex { get; set; }
    public string? Estado { get; set; }
}

// ─────────────────────────────────────────────
// TALLAS
// ─────────────────────────────────────────────
[ApiController]
[Route("api/tallas")]
[Authorize]
public class TallasController : ControllerBase
{
    private readonly AppDbContext _db;
    public TallasController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Tallas.OrderBy(t => t.Orden).ThenBy(t => t.Nombre).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Tallas.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Talla no encontrada"));
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TallaDto dto)
    {
        if (await _db.Tallas.AnyAsync(t => t.Nombre == dto.Nombre))
            return BadRequest(ApiResponse<object>.Fail("Ya existe una talla con ese nombre"));
        var item = new Talla { Nombre = dto.Nombre, Orden = dto.Orden };
        _db.Tallas.Add(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TallaDto dto)
    {
        var item = await _db.Tallas.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Talla no encontrada"));
        item.Nombre = dto.Nombre;
        item.Orden = dto.Orden;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Tallas.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Talla no encontrada"));
        _db.Tallas.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok("Talla eliminada"));
    }
}

public class TallaDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; } = 0;
}

// ─────────────────────────────────────────────
// MARCAS
// ─────────────────────────────────────────────
[ApiController]
[Route("api/marcas")]
[Authorize]
public class MarcasController : ControllerBase
{
    private readonly AppDbContext _db;
    public MarcasController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Marcas.OrderBy(m => m.Nombre).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Marcas.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Marca no encontrada"));
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MarcaDto dto)
    {
        if (await _db.Marcas.AnyAsync(m => m.Nombre == dto.Nombre))
            return BadRequest(ApiResponse<object>.Fail("Ya existe una marca con ese nombre"));
        var item = new Marca { Nombre = dto.Nombre, Logo = dto.Logo, SitioWeb = dto.SitioWeb, Estado = "activo" };
        _db.Marcas.Add(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MarcaDto dto)
    {
        var item = await _db.Marcas.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Marca no encontrada"));
        item.Nombre = dto.Nombre;
        item.Logo = dto.Logo;
        item.SitioWeb = dto.SitioWeb;
        if (dto.Estado != null) item.Estado = dto.Estado;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Marcas.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Marca no encontrada"));
        _db.Marcas.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok("Marca eliminada"));
    }
}

public class MarcaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? SitioWeb { get; set; }
    public string? Estado { get; set; }
}

// ─────────────────────────────────────────────
// MATERIALES
// ─────────────────────────────────────────────
[ApiController]
[Route("api/materiales")]
[Authorize]
public class MaterialesController : ControllerBase
{
    private readonly AppDbContext _db;
    public MaterialesController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Materiales.OrderBy(m => m.Nombre).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Materiales.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Material no encontrado"));
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MaterialDto dto)
    {
        if (await _db.Materiales.AnyAsync(m => m.Nombre == dto.Nombre))
            return BadRequest(ApiResponse<object>.Fail("Ya existe un material con ese nombre"));
        var item = new Material { Nombre = dto.Nombre, Descripcion = dto.Descripcion, Estado = "activo" };
        _db.Materiales.Add(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MaterialDto dto)
    {
        var item = await _db.Materiales.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Material no encontrado"));
        item.Nombre = dto.Nombre;
        item.Descripcion = dto.Descripcion;
        if (dto.Estado != null) item.Estado = dto.Estado;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Materiales.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Material no encontrado"));
        _db.Materiales.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok("Material eliminado"));
    }
}

public class MaterialDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Estado { get; set; }
}

// ─────────────────────────────────────────────
// CATEGORÍAS
// ─────────────────────────────────────────────
[ApiController]
[Route("api/categorias")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriasController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.CategoriasPrincipales.OrderBy(c => c.Nombre).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.CategoriasPrincipales.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Categoría no encontrada"));
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoriaDto dto)
    {
        if (await _db.CategoriasPrincipales.AnyAsync(c => c.Nombre == dto.Nombre))
            return BadRequest(ApiResponse<object>.Fail("Ya existe una categoría con ese nombre"));
        var item = new CategoriaPrincipal { Nombre = dto.Nombre, Descripcion = dto.Descripcion, Imagen = dto.Imagen, Estado = "activo" };
        _db.CategoriasPrincipales.Add(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoriaDto dto)
    {
        var item = await _db.CategoriasPrincipales.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Categoría no encontrada"));
        item.Nombre = dto.Nombre;
        item.Descripcion = dto.Descripcion;
        item.Imagen = dto.Imagen;
        if (dto.Estado != null) item.Estado = dto.Estado;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.CategoriasPrincipales.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Categoría no encontrada"));
        _db.CategoriasPrincipales.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok("Categoría eliminada"));
    }
}

public class CategoriaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Imagen { get; set; }
    public string? Estado { get; set; }
}

// ─────────────────────────────────────────────
// TIPOS DE PRODUCTO
// ─────────────────────────────────────────────
[ApiController]
[Route("api/tipos-producto")]
[Authorize]
public class TiposProductoController : ControllerBase
{
    private readonly AppDbContext _db;
    public TiposProductoController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.TiposProducto.OrderBy(t => t.Nombre).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.TiposProducto.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Tipo de producto no encontrado"));
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TipoProductoDto dto)
    {
        if (await _db.TiposProducto.AnyAsync(t => t.Nombre == dto.Nombre))
            return BadRequest(ApiResponse<object>.Fail("Ya existe un tipo con ese nombre"));
        var item = new TipoProducto { Nombre = dto.Nombre, Descripcion = dto.Descripcion, Estado = "activo" };
        _db.TiposProducto.Add(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TipoProductoDto dto)
    {
        var item = await _db.TiposProducto.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Tipo de producto no encontrado"));
        item.Nombre = dto.Nombre;
        item.Descripcion = dto.Descripcion;
        if (dto.Estado != null) item.Estado = dto.Estado;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.TiposProducto.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Tipo de producto no encontrado"));
        _db.TiposProducto.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok("Tipo eliminado"));
    }
}

public class TipoProductoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Estado { get; set; }
}

// ─────────────────────────────────────────────
// PROVEEDORES
// ─────────────────────────────────────────────
[ApiController]
[Route("api/proveedores")]
[Authorize]
public class ProveedoresController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProveedoresController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Proveedores.OrderBy(p => p.Nombre).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Proveedores
            .Include(p => p.Compras)
            .FirstOrDefaultAsync(p => p.ProveedorID == id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Proveedor no encontrado"));
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProveedorDto dto)
    {
        var item = new Proveedor
        {
            Nombre = dto.Nombre,
            Contacto = dto.Contacto,
            Email = dto.Email,
            Telefono = dto.Telefono,
            Documento = dto.Documento,
            Estado = "activo"
        };
        _db.Proveedores.Add(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProveedorDto dto)
    {
        var item = await _db.Proveedores.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Proveedor no encontrado"));
        item.Nombre = dto.Nombre;
        item.Contacto = dto.Contacto;
        item.Email = dto.Email;
        item.Telefono = dto.Telefono;
        item.Documento = dto.Documento;
        if (dto.Estado != null) item.Estado = dto.Estado;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Proveedores.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Proveedor no encontrado"));
        _db.Proveedores.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok("Proveedor eliminado"));
    }
}

public class ProveedorDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Documento { get; set; }
    public string? Estado { get; set; }
}

// ─────────────────────────────────────────────
// COMPRAS
// ─────────────────────────────────────────────
[ApiController]
[Route("api/compras")]
[Authorize]
public class ComprasController : ControllerBase
{
    private readonly AppDbContext _db;
    public ComprasController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.Detalles)
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.CompraID == id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Compra no encontrada"));
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CompraDto dto)
    {
        var proveedor = await _db.Proveedores.FindAsync(dto.ProveedorID);
        if (proveedor == null) return BadRequest(ApiResponse<object>.Fail("Proveedor no encontrado"));

        var compra = new Compra
        {
            ProveedorID = dto.ProveedorID,
            OrdenFactura = dto.OrdenFactura,
            Fecha = dto.Fecha ?? DateTime.Now,
            Total = dto.Total,
            Estado = "Activa",
            Notas = dto.Notas
        };
        _db.Compras.Add(compra);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(compra));
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> UpdateEstado(int id, [FromBody] EstadoDto dto)
    {
        var item = await _db.Compras.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Compra no encontrada"));
        item.Estado = dto.Estado;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Compras.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Compra no encontrada"));
        _db.Compras.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok("Compra eliminada"));
    }
}

public class CompraDto
{
    public int ProveedorID { get; set; }
    public string OrdenFactura { get; set; } = string.Empty;
    public DateTime? Fecha { get; set; }
    public decimal Total { get; set; }
    public string? Notas { get; set; }
}

// ─────────────────────────────────────────────
// VENTAS
// ─────────────────────────────────────────────
[ApiController]
[Route("api/ventas")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly AppDbContext _db;
    public VentasController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? estado = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        var query = _db.Ventas
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .AsQueryable();

        if (!string.IsNullOrEmpty(estado)) query = query.Where(v => v.Estado == estado);
        if (desde.HasValue) query = query.Where(v => v.FechaVenta >= desde.Value);
        if (hasta.HasValue) query = query.Where(v => v.FechaVenta <= hasta.Value);

        var items = await query.OrderByDescending(v => v.FechaVenta).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Ventas
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.VentaID == id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Venta no encontrada"));
        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VentaDto dto)
    {
        var venta = new Venta
        {
            ClienteID = dto.ClienteID,
            UsuarioID = dto.UsuarioID,
            Subtotal = dto.Subtotal,
            Descuento = dto.Descuento,
            Envio = dto.Envio,
            Total = dto.Total,
            Estado = "Pendiente",
            FechaVenta = DateTime.Now
        };
        _db.Ventas.Add(venta);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(venta));
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> UpdateEstado(int id, [FromBody] EstadoDto dto)
    {
        var item = await _db.Ventas.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Venta no encontrada"));
        item.Estado = dto.Estado;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(item));
    }
}

public class VentaDto
{
    public int? ClienteID { get; set; }
    public int? UsuarioID { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; } = 0;
    public decimal Envio { get; set; } = 0;
    public decimal Total { get; set; }
}

public class EstadoDto
{
    public string Estado { get; set; } = string.Empty;
}