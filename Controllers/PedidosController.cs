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
    private readonly IWebHostEnvironment _env;

    public PedidosController(AppDbContext db, IEmailService email, INotificationService notif, IConfiguration config, IWebHostEnvironment env)
    {
        _db = db; _email = email; _notif = notif; _config = config; _env = env;
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
        var userId = User.GetUserId();

        // Venta manual por admin/empleado
        if (PermissionHelper.HasPermission(User, "ventas:crear") && dto.Items.Count > 0)
        {
            var productosManual = new Dictionary<int, Producto>();
            foreach (var item in dto.Items)
            {
                var prod = await _db.Productos.FindAsync(item.ProductoID);
                if (prod == null) return BadRequest(ApiResponse<object>.Fail($"Producto {item.ProductoID} no encontrado"));
                if (prod.Stock < item.Cantidad) return BadRequest(ApiResponse<object>.Fail($"Stock insuficiente para {prod.Nombre}"));
                productosManual[item.ProductoID] = prod;
            }

            var subtotalManual = dto.Items.Sum(i =>
            {
                var pm = productosManual[i.ProductoID];
                var pr = (i.PrecioUnitario.HasValue && i.PrecioUnitario > 0) ? i.PrecioUnitario.Value : (pm.PrecioOferta ?? pm.PrecioVenta);
                return pr * i.Cantidad;
            });

            var pedidoManual = new Pedido
            {
                ClienteID = userId,
                NombreCliente = dto.NombreCliente,
                EmailCliente = dto.EmailCliente,
                TelefonoCliente = dto.TelefonoCliente,
                DocumentoCliente = dto.DocumentoCliente,
                DireccionEnvio = dto.DireccionEnvio,
                Ciudad = dto.Ciudad,
                MetodoPago = dto.MetodoPago,
                Notas = dto.Notas,
                ComprobantePago = dto.ComprobantePago,
                Subtotal = subtotalManual,
                Total = subtotalManual,
                Estado = "Aprobado",
                FechaPedido = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow
            };

            using var txManual = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Pedidos.Add(pedidoManual);
                await _db.SaveChangesAsync();

                foreach (var item in dto.Items)
                {
                    var prod = productosManual[item.ProductoID];
                    var precio = (item.PrecioUnitario.HasValue && item.PrecioUnitario > 0)
                        ? item.PrecioUnitario.Value
                        : (prod.PrecioOferta ?? prod.PrecioVenta);
                    _db.PedidoDetalles.Add(new PedidoDetalle
                    {
                        PedidoID = pedidoManual.PedidoID,
                        ProductoID = item.ProductoID,
                        TallaID = item.TallaID,
                        ColorID = item.ColorID,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = precio,
                        Subtotal = precio * item.Cantidad
                    });
                    prod.Stock -= item.Cantidad;
                    _db.StockMovimientos.Add(new StockMovimiento
                    {
                        ProductoID = item.ProductoID,
                        Cantidad = -item.Cantidad,
                        Tipo = "salida",
                        ReferenciaTipo = "venta_manual",
                        ReferenciaID = pedidoManual.PedidoID,
                        UsuarioID = userId,
                        Fecha = DateTime.UtcNow
                    });
                }

                await _db.SaveChangesAsync();
                await txManual.CommitAsync();
            }
            catch
            {
                await txManual.RollbackAsync();
                throw;
            }

            _ = _email.SendOrderConfirmationClienteAsync(pedidoManual.EmailCliente, pedidoManual.NombreCliente, pedidoManual.PedidoID, pedidoManual.Total);
            var adminEmailManual = _config["Email:FromEmail"];
            if (!string.IsNullOrEmpty(adminEmailManual))
                _ = _email.SendOrderConfirmationAdminAsync(adminEmailManual, pedidoManual.NombreCliente, pedidoManual.PedidoID, pedidoManual.Total);

            return CreatedAtAction(nameof(GetById), new { id = pedidoManual.PedidoID },
                ApiResponse<object>.Ok(new { pedidoId = pedidoManual.PedidoID, total = pedidoManual.Total }, "Venta registrada"));
        }

        // Checkout de cliente
        if (!PermissionHelper.HasPermission(User, "tienda:comprar"))
            return Forbid();

        var usuario = await _db.Usuarios.FindAsync(userId);
        if (usuario == null) return Unauthorized();

        var nombreC = !string.IsNullOrWhiteSpace(dto.NombreCliente) ? dto.NombreCliente : usuario.NombreCompleto;
        var emailC  = !string.IsNullOrWhiteSpace(dto.EmailCliente)  ? dto.EmailCliente  : usuario.Email;
        var telC    = !string.IsNullOrWhiteSpace(dto.TelefonoCliente) ? dto.TelefonoCliente : (usuario.Telefono ?? "");

        // Preferir Items enviados en el request; usar carrito DB como fallback
        var itemsCheckout = new List<(int ProdID, Producto Prod, int Cant, int? TallaID, int? ColorID)>();
        List<Carrito>? dbCart = null;

        if (dto.Items.Count > 0)
        {
            foreach (var it in dto.Items)
            {
                var prod = await _db.Productos.FindAsync(it.ProductoID);
                if (prod == null) return BadRequest(ApiResponse<object>.Fail($"Producto {it.ProductoID} no encontrado"));
                if (prod.Stock < it.Cantidad) return BadRequest(ApiResponse<object>.Fail($"Stock insuficiente para {prod.Nombre}"));
                itemsCheckout.Add((it.ProductoID, prod, it.Cantidad, it.TallaID, it.ColorID));
            }
        }
        else
        {
            dbCart = await _db.Carrito.Include(c => c.Producto).Where(c => c.UsuarioID == userId).ToListAsync();
            if (!dbCart.Any()) return BadRequest(ApiResponse<object>.Fail("El carrito esta vacio"));
            foreach (var ci in dbCart)
            {
                if (ci.Producto.Stock < ci.Cantidad)
                    return BadRequest(ApiResponse<object>.Fail($"Stock insuficiente para {ci.Producto.Nombre}"));
                itemsCheckout.Add((ci.ProductoID, ci.Producto, ci.Cantidad, null, null));
            }
        }

        var subtotal = itemsCheckout.Sum(it => (it.Prod.PrecioOferta ?? it.Prod.PrecioVenta) * it.Cant);

        var pedido = new Pedido
        {
            ClienteID = userId,
            NombreCliente = nombreC,
            EmailCliente = emailC,
            TelefonoCliente = telC,
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
            Total = subtotal,
            Notas = dto.Notas,
            ComprobantePago = dto.ComprobantePago,
            FechaPedido = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Pedidos.Add(pedido);
            await _db.SaveChangesAsync();

            foreach (var it in itemsCheckout)
            {
                var precio = it.Prod.PrecioOferta ?? it.Prod.PrecioVenta;
                _db.PedidoDetalles.Add(new PedidoDetalle
                {
                    PedidoID = pedido.PedidoID,
                    ProductoID = it.ProdID,
                    TallaID = it.TallaID,
                    ColorID = it.ColorID,
                    Cantidad = it.Cant,
                    PrecioUnitario = precio,
                    Subtotal = precio * it.Cant
                });
                it.Prod.Stock -= it.Cant;
                _db.StockMovimientos.Add(new StockMovimiento
                {
                    ProductoID = it.ProdID,
                    Cantidad = -it.Cant,
                    Tipo = "salida",
                    ReferenciaTipo = "pedido",
                    ReferenciaID = pedido.PedidoID,
                    UsuarioID = userId,
                    Fecha = DateTime.UtcNow
                });
            }

            if (dbCart != null && dbCart.Any())
                _db.Carrito.RemoveRange(dbCart);
            else
            {
                var cartExtra = await _db.Carrito.Where(c => c.UsuarioID == userId).ToListAsync();
                if (cartExtra.Any()) _db.Carrito.RemoveRange(cartExtra);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        await _notif.CreateAsync(userId, "Pedido creado",
            $"Tu pedido #{pedido.PedidoID} por ${subtotal:F2} ha sido recibido.", "success", $"pedido:{pedido.PedidoID}");

        _ = _email.SendOrderConfirmationClienteAsync(emailC, nombreC, pedido.PedidoID, subtotal);
        var adminEmail = _config["Email:FromEmail"];
        if (!string.IsNullOrEmpty(adminEmail))
            _ = _email.SendOrderConfirmationAdminAsync(adminEmail, nombreC, pedido.PedidoID, subtotal);

        return CreatedAtAction(nameof(GetById), new { id = pedido.PedidoID },
            ApiResponse<object>.Ok(new { pedidoId = pedido.PedidoID, total = subtotal }, "Pedido creado"));
    }

    [HttpPut("{id}/estado")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateEstado(int id, [FromBody] ActualizarEstadoPedidoDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "ventas:editar"))
            return Forbid();

        var pedido = await _db.Pedidos.Include(p => p.Cliente).FirstOrDefaultAsync(p => p.PedidoID == id);
        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));

        var estadosValidos = new[] { "Pendiente", "Aprobada", "Aprobado", "En proceso", "Enviado", "Entregado", "Cancelado", "Rechazada", "Rechazado", "Completada", "Completado" };
        if (!estadosValidos.Contains(dto.NuevoEstado))
            return BadRequest(ApiResponse<object>.Fail("Estado invalido"));

        // Restaurar stock si se cancela o rechaza un pedido que ya tenía stock descontado
        var estadosQueDescontanStock = new[] { "Aprobado", "Aprobada", "En proceso", "Enviado", "Entregado", "Completado", "Completada" };
        var estadosCancelacion = new[] { "Cancelado", "Rechazado", "Rechazada" };
        if (estadosCancelacion.Contains(dto.NuevoEstado) && estadosQueDescontanStock.Contains(pedido.Estado))
        {
            var detalles = await _db.PedidoDetalles.Where(d => d.PedidoID == id).ToListAsync();
            foreach (var det in detalles)
            {
                var prod = await _db.Productos.FindAsync(det.ProductoID);
                if (prod != null) prod.Stock += det.Cantidad;
            }
        }

        pedido.Estado = dto.NuevoEstado;
        if (dto.NumeroGuia != null) pedido.NumeroGuia = dto.NumeroGuia;
        if (dto.Transportadora != null) pedido.Transportadora = dto.Transportadora;
        if (dto.Notas != null) pedido.Notas = dto.Notas;
        if (dto.NuevoEstado == "Enviado") pedido.FechaEnvio = DateTime.UtcNow;
        if (dto.NuevoEstado == "Entregado") pedido.FechaEntrega = DateTime.UtcNow;
        pedido.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _notif.CreateAsync(pedido.ClienteID, "Pedido actualizado",
            $"Tu pedido #{id} ahora esta: {dto.NuevoEstado}", "info", $"pedido:{id}");

        _ = _email.SendOrderStatusUpdateAsync(pedido.EmailCliente, pedido.NombreCliente, id, dto.NuevoEstado);
        return Ok(ApiResponse<object>.Ok(new { estado = dto.NuevoEstado }, "Estado actualizado"));
    }

    [HttpPost("{id}/email-pago")]
    public async Task<ActionResult<ApiResponse<object>>> EnviarEmailPago(int id, [FromBody] EnviarEmailPagoDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "pedidos:editar"))
            return Forbid();

        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.PedidoID == id);
        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));

        var banco = _config["BankAccount:Banco"] ?? "";
        var cuenta = _config["BankAccount:NumeroCuenta"] ?? "";
        var titular = _config["BankAccount:Titular"] ?? "";
        var tipoCuenta = _config["BankAccount:TipoCuenta"] ?? "";

        _ = _email.SendPaymentInfoEmailAsync(
            pedido.EmailCliente, pedido.NombreCliente, pedido.PedidoID, pedido.Total,
            banco, cuenta, titular, tipoCuenta, dto.Mensaje);

        return Ok(ApiResponse<object>.Ok(new { }, "Correo de pago enviado"));
    }

    [HttpPost("{id}/email-guia")]
    public async Task<ActionResult<ApiResponse<object>>> EnviarEmailGuia(int id, [FromForm] EnviarEmailGuiaDto dto)
    {
        if (!PermissionHelper.HasPermission(User, "pedidos:editar"))
            return Forbid();

        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.PedidoID == id);
        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));

        string? fotoUrl = null;
        if (dto.Foto != null && dto.Foto.Length > 0)
        {
            var uploadsPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            Directory.CreateDirectory(uploadsPath);
            var fileName = $"guia_{Guid.NewGuid()}{Path.GetExtension(dto.Foto.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await dto.Foto.CopyToAsync(stream);
            var baseUrl = _config["AppSettings:BaseUrl"]?.TrimEnd('/') ?? "";
            fotoUrl = string.IsNullOrEmpty(baseUrl) ? $"/uploads/{fileName}" : $"{baseUrl}/uploads/{fileName}";
        }

        if (!string.IsNullOrEmpty(dto.NumeroGuia)) pedido.NumeroGuia = dto.NumeroGuia;
        if (!string.IsNullOrEmpty(dto.Transportadora)) pedido.Transportadora = dto.Transportadora;
        var estadosTerminales = new[] { "Entregado", "Cancelado", "Rechazado", "Rechazada", "Completado", "Completada" };
        if (!estadosTerminales.Contains(pedido.Estado))
        {
            pedido.Estado = "Enviado";
            pedido.FechaEnvio = DateTime.UtcNow;
            pedido.FechaActualizacion = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();

        _ = _email.SendShippingEmailAsync(pedido.EmailCliente, pedido.NombreCliente, pedido.PedidoID, dto.NumeroGuia, dto.Transportadora, fotoUrl);

        return Ok(ApiResponse<object>.Ok(new { }, "Correo de envío enviado"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var userId = User.GetUserId();
        var hasDelete = PermissionHelper.HasPermission(User, "ventas:eliminar");

        var pedido = await _db.Pedidos
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.PedidoID == id);
        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));
        if (!hasDelete && pedido.ClienteID != userId) return Forbid();

        if (hasDelete)
        {
            // Admin: elimina el registro permanentemente del historial
            var movimientos = await _db.StockMovimientos
                .Where(m => m.ReferenciaTipo == "pedido" || m.ReferenciaTipo == "venta_manual")
                .Where(m => m.ReferenciaID == id)
                .ToListAsync();
            _db.StockMovimientos.RemoveRange(movimientos);
            _db.PedidoDetalles.RemoveRange(pedido.Detalles);
            _db.Pedidos.Remove(pedido);
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { }, "Registro eliminado"));
        }

        // Cliente: solo puede cancelar si no está entregado
        if (pedido.Estado == "Entregado" || pedido.Estado == "Completada" || pedido.Estado == "Completado")
            return BadRequest(ApiResponse<object>.Fail("No se puede cancelar un pedido entregado"));

        pedido.Estado = "Cancelado";
        pedido.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { }, "Pedido cancelado"));
    }

    [HttpPost("{id}/comprobante")]
    public async Task<ActionResult<ApiResponse<object>>> SubirComprobante(int id, [FromForm] IFormFile archivo)
    {
        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.PedidoID == id);
        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));
        if (archivo == null || archivo.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Archivo vacío"));

        var uploadsPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
        Directory.CreateDirectory(uploadsPath);
        var ext = Path.GetExtension(archivo.FileName);
        var fileName = $"comprobante_{id}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await archivo.CopyToAsync(stream);

        var baseUrl = _config["AppSettings:BaseUrl"]?.TrimEnd('/') ?? "";
        var url = string.IsNullOrEmpty(baseUrl) ? $"/uploads/{fileName}" : $"{baseUrl}/uploads/{fileName}";

        pedido.ComprobantePago = url;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { url }, "Comprobante subido"));
    }

    private static PedidoDto MapToDto(Pedido p) => new()
    {
        PedidoID = p.PedidoID, ClienteID = p.ClienteID, FechaPedido = p.FechaPedido,
        NombreCliente = p.NombreCliente, EmailCliente = p.EmailCliente, TelefonoCliente = p.TelefonoCliente,
        DireccionEnvio = p.DireccionEnvio, Ciudad = p.Ciudad, MetodoPago = p.MetodoPago,
        Subtotal = p.Subtotal, Descuento = p.Descuento, Envio = p.Envio, Total = p.Total,
        Estado = p.Estado, NumeroGuia = p.NumeroGuia, Transportadora = p.Transportadora,
        ComprobantePago = p.ComprobantePago,
        Detalles = p.Detalles?.Select(d => new PedidoDetalleDto
        {
            ProductoID = d.ProductoID, ProductoNombre = d.Producto?.Nombre ?? "",
            Talla = d.Talla?.Nombre, Color = d.Color?.Nombre,
            Cantidad = d.Cantidad, PrecioUnitario = d.PrecioUnitario, Subtotal = d.Subtotal
        }).ToList() ?? new()
    };
}
