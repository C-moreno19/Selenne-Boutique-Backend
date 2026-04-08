using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.DTOs.Pedidos;
using SelenneApi.Models.Entities;
using SelenneApi.Services;
using SelenneApi.Helpers;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/pedidos")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly INotificationService _notif;
    private readonly IConfiguration _config;

    public PedidosController(AppDbContext db, IEmailService email, INotificationService notif, IConfiguration config)
    {
        _db = db; _email = email; _notif = notif; _config = config;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<PedidoDto>>>> GetAll()
    {
        var items = await _db.Pedidos
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .Include(p => p.Detalles).ThenInclude(d => d.Talla)
            .Include(p => p.Detalles).ThenInclude(d => d.Color)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();
        return Ok(ApiResponse<List<PedidoDto>>.Ok(items.Select(MapToDto).ToList()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PedidoDto>>> GetById(int id)
    {
        var userId = User.GetUserId();
        var hasVer = PermissionHelper.HasPermission(User, "ventas:ver");

        var pedido = await _db.Pedidos
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .Include(p => p.Detalles).ThenInclude(d => d.Talla)
            .Include(p => p.Detalles).ThenInclude(d => d.Color)
            .FirstOrDefaultAsync(p => p.PedidoID == id);

        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));
        if (!hasVer && pedido.ClienteID != userId)
            return Forbid();

        return Ok(ApiResponse<PedidoDto>.Ok(MapToDto(pedido)));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CrearPedidoRequestDto dto)
    {
        // Obtener userId si está autenticado
        int userId = 0;
        try { userId = User.GetUserId(); } catch { }

        // Usar items del DTO directamente
        if (dto.Items == null || !dto.Items.Any())
            return BadRequest(ApiResponse<object>.Fail("Debes incluir al menos un producto"));

        // Validar stock y obtener productos
        var productoIds = dto.Items.Select(i => i.ProductoID).ToList();
        var productos = await _db.Productos.Where(p => productoIds.Contains(p.ProductoID)).ToListAsync();

        foreach (var item in dto.Items)
        {
            var producto = productos.FirstOrDefault(p => p.ProductoID == item.ProductoID);
            if (producto == null) return BadRequest(ApiResponse<object>.Fail($"Producto {item.ProductoID} no encontrado"));
            if (producto.Stock < item.Cantidad) return BadRequest(ApiResponse<object>.Fail($"Stock insuficiente para {producto.Nombre}"));
        }

        var subtotal = dto.Items.Sum(i => {
            var p = productos.First(p => p.ProductoID == i.ProductoID);
            return (p.PrecioOferta ?? p.PrecioVenta) * i.Cantidad;
        });

        var pedido = new Pedido
        {
            ClienteID = userId > 0 ? userId : 1,
            NombreCliente = dto.NombreCliente,
            EmailCliente = dto.EmailCliente,
            TelefonoCliente = dto.TelefonoCliente,
            DocumentoCliente = dto.DocumentoCliente,
            DireccionEnvio = dto.DireccionEnvio,
            Ciudad = dto.Ciudad,
            CodigoPostal = dto.CodigoPostal,
            MetodoPago = dto.MetodoPago,
            NumeroCuenta = dto.NumeroCuenta,
            NombreTitular = dto.NombreTitular,
            Banco = dto.Banco,
            TipoCuenta = dto.TipoCuenta,
            Subtotal = subtotal,
            Total = subtotal,
            Notas = dto.Notas,
            FechaPedido = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Pedidos.Add(pedido);
            await _db.SaveChangesAsync();

            foreach (var item in dto.Items)
            {
                var producto = productos.First(p => p.ProductoID == item.ProductoID);
                var precio = producto.PrecioOferta ?? producto.PrecioVenta;
                _db.PedidoDetalles.Add(new PedidoDetalle
                {
                    PedidoID = pedido.PedidoID,
                    ProductoID = item.ProductoID,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precio,
                    Subtotal = precio * item.Cantidad,
                    TallaID = item.TallaID,
                    ColorID = item.ColorID,
                });
                producto.Stock -= item.Cantidad;
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return CreatedAtAction(nameof(GetById), new { id = pedido.PedidoID },
            ApiResponse<object>.Ok(new { pedidoId = pedido.PedidoID, total = pedido.Total }, "Pedido creado"));
    }

    [HttpPut("{id}/estado")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> UpdateEstado(int id, [FromBody] ActualizarEstadoPedidoDto dto)
    {
        var pedido = await _db.Pedidos.Include(p => p.Cliente).FirstOrDefaultAsync(p => p.PedidoID == id);
        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));

        var estadosValidos = new[] { "Pendiente", "Aprobado", "En Proceso", "Completado", "Cancelado", "Rechazado" };
        if (!estadosValidos.Contains(dto.NuevoEstado))
            return BadRequest(ApiResponse<object>.Fail("Estado invalido"));

        pedido.Estado = dto.NuevoEstado;
        if (dto.NumeroGuia != null) pedido.NumeroGuia = dto.NumeroGuia;
        if (dto.Transportadora != null) pedido.Transportadora = dto.Transportadora;
        if (dto.Notas != null) pedido.Notas = dto.Notas;
        pedido.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { estado = dto.NuevoEstado }, "Estado actualizado"));
    }

    [HttpPost("{id}/comprobante")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> SubirComprobante(int id, IFormFile archivo)
    {
        var pedido = await _db.Pedidos.FindAsync(id);
        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

        var ext = Path.GetExtension(archivo.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await archivo.CopyToAsync(stream);

        pedido.ComprobantePago = $"/uploads/{fileName}";
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { url = pedido.ComprobantePago }));
    }

    [HttpDelete("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var pedido = await _db.Pedidos.Include(p => p.Detalles).FirstOrDefaultAsync(p => p.PedidoID == id);
        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));
        _db.PedidoDetalles.RemoveRange(pedido.Detalles);
        _db.Pedidos.Remove(pedido);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok("Pedido eliminado"));
    }

    private static PedidoDto MapToDto(Pedido p) => new()
    {
        PedidoID = p.PedidoID,
        ClienteID = p.ClienteID,
        FechaPedido = p.FechaPedido,
        NombreCliente = p.NombreCliente,
        EmailCliente = p.EmailCliente,
        TelefonoCliente = p.TelefonoCliente,
        DireccionEnvio = p.DireccionEnvio,
        Ciudad = p.Ciudad,
        MetodoPago = p.MetodoPago,
        Subtotal = p.Subtotal,
        Descuento = p.Descuento,
        Envio = p.Envio,
        Total = p.Total,
        Estado = p.Estado,
        NumeroGuia = p.NumeroGuia,
        Transportadora = p.Transportadora,
        ComprobantePago = p.ComprobantePago,
        Notas = p.Notas,
        Detalles = p.Detalles?.Select(d => new PedidoDetalleDto
        {
            ProductoID = d.ProductoID,
            ProductoNombre = d.Producto?.Nombre ?? "",
            Talla = d.Talla?.Nombre,
            Color = d.Color?.Nombre,
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            Subtotal = d.Subtotal
        }).ToList() ?? new()
    };
}