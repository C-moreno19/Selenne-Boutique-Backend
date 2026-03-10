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
    public async Task<ActionResult<ApiResponse<List<PedidoDto>>>> GetAll()
    {
        var userId = User.GetUserId();
        var hasVer = PermissionHelper.HasPermission(User, "ventas:ver");

        IQueryable<Pedido> query = _db.Pedidos
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .Include(p => p.Detalles).ThenInclude(d => d.Talla)
            .Include(p => p.Detalles).ThenInclude(d => d.Color);

        if (!hasVer)
            query = query.Where(p => p.ClienteID == userId);

        var pedidos = await query.OrderByDescending(p => p.FechaPedido).ToListAsync();
        return Ok(ApiResponse<List<PedidoDto>>.Ok(pedidos.Select(MapToDto).ToList()));
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
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CrearPedidoRequestDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "tienda:comprar"))
            return Forbid();

        var userId = User.GetUserId();
        var usuario = await _db.Usuarios.FindAsync(userId);
        if (usuario == null) return Unauthorized();

        var carritoItems = await _db.Carrito
            .Include(c => c.Producto)
            .Where(c => c.UsuarioID == userId)
            .ToListAsync();

        if (!carritoItems.Any())
            return BadRequest(ApiResponse<object>.Fail("El carrito esta vacio"));

        // Validate stock
        foreach (var item in carritoItems)
        {
            if (item.Producto.Stock < item.Cantidad)
                return BadRequest(ApiResponse<object>.Fail($"Stock insuficiente para {item.Producto.Nombre}"));
        }

        var subtotal = carritoItems.Sum(c => (c.Producto.PrecioOferta ?? c.Producto.PrecioVenta) * c.Cantidad);
        var total = subtotal;

        var pedido = new Pedido
        {
            ClienteID = userId,
            NombreCliente = usuario.NombreCompleto,
            EmailCliente = usuario.Email,
            TelefonoCliente = usuario.Telefono ?? dto.DireccionEnvio,
            DocumentoCliente = usuario.Documento,
            DireccionEnvio = dto.DireccionEnvio,
            Ciudad = dto.Ciudad,
            CodigoPostal = dto.CodigoPostal,
            MetodoPago = dto.MetodoPago,
            NumeroCuenta = dto.NumeroCuenta,
            NombreTitular = dto.NombreTitular,
            Banco = dto.Banco,
            TipoCuenta = dto.TipoCuenta,
            Subtotal = subtotal,
            Total = total,
            Notas = dto.Notas,
            FechaPedido = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Pedidos.Add(pedido);
            await _db.SaveChangesAsync();

            // Add details and reduce stock
            foreach (var item in carritoItems)
            {
                var precio = item.Producto.PrecioOferta ?? item.Producto.PrecioVenta;
                _db.PedidoDetalles.Add(new PedidoDetalle
                {
                    PedidoID = pedido.PedidoID,
                    ProductoID = item.ProductoID,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precio,
                    Subtotal = precio * item.Cantidad
                });

                item.Producto.Stock -= item.Cantidad;

                _db.StockMovimientos.Add(new StockMovimiento
                {
                    ProductoID = item.ProductoID,
                    Cantidad = -item.Cantidad,
                    Tipo = "salida",
                    ReferenciaTipo = "pedido",
                    ReferenciaID = pedido.PedidoID,
                    UsuarioID = userId,
                    Fecha = DateTime.Now
                });
            }

            // Clear cart
            _db.Carrito.RemoveRange(carritoItems);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        // Notifications and emails
        await _notif.CreateAsync(userId, "Pedido creado",
            $"Tu pedido #{pedido.PedidoID} por ${total:F2} ha sido recibido.", "success", $"pedido:{pedido.PedidoID}");

        _ = _email.SendOrderConfirmationClienteAsync(usuario.Email, usuario.NombreCompleto, pedido.PedidoID, total);

        // Notify admins
        var adminEmail = _config["Email:FromEmail"];
        if (!string.IsNullOrEmpty(adminEmail))
            _ = _email.SendOrderConfirmationAdminAsync(adminEmail, usuario.NombreCompleto, pedido.PedidoID, total);

        return CreatedAtAction(nameof(GetById), new { id = pedido.PedidoID },
            ApiResponse<object>.Ok(new { pedidoId = pedido.PedidoID, total }, "Pedido creado"));
    }

    [HttpPut("{id}/estado")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateEstado(int id, [FromBody] ActualizarEstadoPedidoDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "ventas:editar"))
            return Forbid();

        var pedido = await _db.Pedidos.Include(p => p.Cliente).FirstOrDefaultAsync(p => p.PedidoID == id);
        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));

        var estadosValidos = new[] { "Pendiente", "Aprobada", "En proceso", "Enviado", "Entregado", "Cancelado", "Rechazada", "Completada" };
        if (!estadosValidos.Contains(dto.NuevoEstado))
            return BadRequest(ApiResponse<object>.Fail("Estado invalido"));

        pedido.Estado = dto.NuevoEstado;
        if (dto.NumeroGuia != null) pedido.NumeroGuia = dto.NumeroGuia;
        if (dto.Transportadora != null) pedido.Transportadora = dto.Transportadora;
        if (dto.Notas != null) pedido.Notas = dto.Notas;
        if (dto.NuevoEstado == "Enviado") pedido.FechaEnvio = DateTime.Now;
        if (dto.NuevoEstado == "Entregado") pedido.FechaEntrega = DateTime.Now;
        pedido.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await _notif.CreateAsync(pedido.ClienteID, "Pedido actualizado",
            $"Tu pedido #{id} ahora esta: {dto.NuevoEstado}", "info", $"pedido:{id}");

        _ = _email.SendOrderStatusUpdateAsync(pedido.Cliente.Email, pedido.Cliente.NombreCompleto, id, dto.NuevoEstado);
        return Ok(ApiResponse<object>.Ok(new { estado = dto.NuevoEstado }, "Estado actualizado"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(int id)
    {
        var userId = User.GetUserId();
        var hasDelete = PermissionHelper.HasPermission(User, "ventas:eliminar");

        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.PedidoID == id);
        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));
        if (!hasDelete && pedido.ClienteID != userId) return Forbid();

        if (pedido.Estado == "Entregado" || pedido.Estado == "Completada")
            return BadRequest(ApiResponse<object>.Fail("No se puede cancelar un pedido entregado"));

        pedido.Estado = "Cancelado";
        pedido.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { }, "Pedido cancelado"));
    }

    private static PedidoDto MapToDto(Pedido p) => new()
    {
        PedidoID = p.PedidoID, ClienteID = p.ClienteID, FechaPedido = p.FechaPedido,
        NombreCliente = p.NombreCliente, EmailCliente = p.EmailCliente, TelefonoCliente = p.TelefonoCliente,
        DireccionEnvio = p.DireccionEnvio, Ciudad = p.Ciudad, MetodoPago = p.MetodoPago,
        Subtotal = p.Subtotal, Descuento = p.Descuento, Envio = p.Envio, Total = p.Total,
        Estado = p.Estado, NumeroGuia = p.NumeroGuia, Transportadora = p.Transportadora,
        Detalles = p.Detalles?.Select(d => new PedidoDetalleDto
        {
            ProductoID = d.ProductoID, ProductoNombre = d.Producto?.Nombre ?? "",
            Talla = d.Talla?.Nombre, Color = d.Color?.Nombre,
            Cantidad = d.Cantidad, PrecioUnitario = d.PrecioUnitario, Subtotal = d.Subtotal
        }).ToList() ?? new()
    };
}
