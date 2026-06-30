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

    [HttpGet("mis-pedidos")]
    public async Task<ActionResult<ApiResponse<List<PedidoDto>>>> GetMisPedidos()
    {
        var userId = User.GetUserId();
        var items = await _db.Pedidos
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .Include(p => p.Detalles).ThenInclude(d => d.Talla)
            .Include(p => p.Detalles).ThenInclude(d => d.Color)
            .Where(p => p.ClienteID == userId)
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
                FechaActualizacion = DateTime.UtcNow,
                ConfirmacionToken = Guid.NewGuid().ToString("N")
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

        // Checkout de cliente — cualquier usuario autenticado puede comprar
        var usuario = await _db.Usuarios.FindAsync(userId);
        if (usuario == null) return Unauthorized(ApiResponse<object>.Fail("Debes iniciar sesión para realizar un pedido"));

        var nombreC = !string.IsNullOrWhiteSpace(dto.NombreCliente) ? dto.NombreCliente : usuario.NombreCompleto;
        var emailC  = !string.IsNullOrWhiteSpace(dto.EmailCliente)  ? dto.EmailCliente  : usuario.Email;
        var telC    = !string.IsNullOrWhiteSpace(dto.TelefonoCliente) ? dto.TelefonoCliente : (usuario.Telefono ?? "");

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
            FechaActualizacion = DateTime.UtcNow,
            ConfirmacionToken = Guid.NewGuid().ToString("N")
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
            $"Tu pedido por ${subtotal:N0} ha sido recibido y está siendo procesado.", "success", $"pedido:{pedido.PedidoID}");

        _ = _email.SendOrderConfirmationClienteAsync(emailC, nombreC, pedido.PedidoID, subtotal);
        var adminEmail = _config["Email:FromEmail"];
        if (!string.IsNullOrEmpty(adminEmail))
            _ = _email.SendOrderConfirmationAdminAsync(adminEmail, nombreC, pedido.PedidoID, subtotal);

        return CreatedAtAction(nameof(GetById), new { id = pedido.PedidoID },
            ApiResponse<object>.Ok(new { pedidoId = pedido.PedidoID, total = subtotal }, "Pedido creado"));
    }

    [HttpPut("{id}/estado")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> UpdateEstado(int id, [FromBody] ActualizarEstadoPedidoDto dto)
    {
        var pedido = await _db.Pedidos.Include(p => p.Cliente).FirstOrDefaultAsync(p => p.PedidoID == id);
        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));

        var estadosValidos = new[] { "Pendiente", "Aprobada", "Aprobado", "En proceso", "Enviado", "Entregado", "Cancelado", "Rechazada", "Rechazado", "Completada", "Completado" };
        if (!estadosValidos.Contains(dto.NuevoEstado))
            return BadRequest(ApiResponse<object>.Fail("Estado invalido"));

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

        if (dto.NuevoEstado == "Aprobado" && !string.IsNullOrEmpty(pedido.EmailCliente))
        {
            var cliente = pedido.ClienteID > 0 ? await _db.Usuarios.FindAsync(pedido.ClienteID) : null;
            if (cliente == null || cliente.NotificacionesEmail)
                _ = _email.SendOrderApprovedAsync(pedido.EmailCliente, pedido.NombreCliente, pedido.PedidoID, pedido.Total);
        }

        var cid = pedido.ClienteID;
        if (cid > 0)
        {
            (string titulo, string mensaje, string tipo) notif = dto.NuevoEstado switch
            {
                "Aprobado"   => ("✅ Pedido aprobado",   "Tu pedido fue aprobado y será enviado en las próximas 72 horas.", "success"),
                "Rechazado"  => ("❌ Pedido rechazado",  "Tu pedido fue rechazado." + (string.IsNullOrEmpty(dto.Notas) ? "" : $" Motivo: {dto.Notas}"), "error"),
                "Completado" => ("🎉 Pedido completado", "Tu pedido fue completado. ¡Gracias por tu compra!", "success"),
                "Cancelado"  => ("Pedido cancelado",     "Tu pedido fue cancelado.", "warning"),
                _ => ("", "", "")
            };
            if (!string.IsNullOrEmpty(notif.titulo))
                _ = _notif.CreateAsync(cid, notif.titulo, notif.mensaje, notif.tipo, $"pedido-{pedido.PedidoID}");
        }

        _ = _email.SendOrderStatusUpdateAsync(pedido.EmailCliente, pedido.NombreCliente, id, dto.NuevoEstado, dto.Notas);
        return Ok(ApiResponse<object>.Ok(new { estado = dto.NuevoEstado }, "Estado actualizado"));
    }

    [HttpGet("confirmar/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmarEntrega(string token)
    {
        string PageHtml(string titulo, string tituloColor, string icono, string mensaje, string subMensaje = "") =>
            "<!DOCTYPE html><html lang='es'><head><meta charset='UTF-8'><meta name='viewport' content='width=device-width,initial-scale=1'><title>Selenne Boutique</title>" +
            "<style>" +
            "*{margin:0;padding:0;box-sizing:border-box}" +
            "body{font-family:'Helvetica Neue',Helvetica,Arial,sans-serif;background:linear-gradient(145deg,#fce7f3 0%,#fff0f7 50%,#fdf2f8 100%);min-height:100vh;display:flex;align-items:center;justify-content:center;padding:24px}" +
            ".card{background:white;border-radius:24px;box-shadow:0 24px 64px rgba(214,83,145,.18);max-width:460px;width:100%;overflow:hidden;text-align:center}" +
            ".header{background:linear-gradient(135deg,#c94f87 0%,#d65391 50%,#e8709f 100%);padding:36px 32px 32px;position:relative}" +
            ".header::after{content:'';position:absolute;bottom:0;left:0;right:0;height:1px;background:rgba(255,255,255,.15)}" +
            ".brand-tag{color:rgba(255,255,255,.6);font-size:10px;font-weight:700;letter-spacing:4px;text-transform:uppercase;margin-bottom:8px}" +
            ".header h1{color:white;font-size:24px;font-weight:700;letter-spacing:.5px}" +
            ".header p{color:rgba(255,255,255,.75);font-size:13px;margin-top:5px;font-weight:400}" +
            ".body{padding:44px 36px 36px}" +
            ".icon-wrap{width:80px;height:80px;border-radius:50%;background:linear-gradient(135deg,#fce7f3,#fdf2f8);border:2px solid #fce7f3;display:flex;align-items:center;justify-content:center;margin:0 auto 20px;font-size:40px}" +
            ".titulo{font-size:20px;font-weight:700;color:" + tituloColor + ";margin-bottom:14px;letter-spacing:-.3px}" +
            ".mensaje{color:#374151;font-size:15px;line-height:1.65;margin-bottom:4px}" +
            ".sub{color:#9ca3af;font-size:13px;margin-top:16px;line-height:1.5}" +
            ".divider{width:40px;height:3px;background:linear-gradient(90deg,#d65391,#f8a9c5);border-radius:2px;margin:24px auto 0}" +
            ".footer{background:#fdf2f8;padding:18px 32px;border-top:1px solid #fce7f3;display:flex;align-items:center;justify-content:center;gap:8px}" +
            ".footer-dot{width:4px;height:4px;border-radius:50%;background:#f9a8d4}" +
            ".footer p{color:#d65391;font-size:12px;font-weight:600;letter-spacing:.5px}" +
            "</style></head>" +
            "<body><div class='card'>" +
            "<div class='header'><p class='brand-tag'>Selenne Boutique</p><h1>Selenne Boutique</h1><p>Moda con estilo</p></div>" +
            "<div class='body'><div class='icon-wrap'>" + icono + "</div>" +
            "<p class='titulo'>" + titulo + "</p><p class='mensaje'>" + mensaje + "</p>" +
            (subMensaje != "" ? "<p class='sub'>" + subMensaje + "</p>" : "") +
            "<div class='divider'></div></div>" +
            "<div class='footer'><div class='footer-dot'></div><p>selenneboutique.com</p><div class='footer-dot'></div></div>" +
            "</div></body></html>";

        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.ConfirmacionToken == token);
        if (pedido == null)
            return Content(PageHtml("Enlace no válido", "#dc2626", "❌", "Este enlace es inválido o ya fue utilizado anteriormente.", "Si crees que es un error, contacta a Selenne Boutique."), "text/html; charset=utf-8");

        if (pedido.Estado == "Completado")
            return Content(PageHtml("Pedido ya confirmado", "#d65391", "✅", "Tu pedido ya fue marcado como completado. ¡Gracias por tu compra!", "Esperamos verte pronto de nuevo."), "text/html; charset=utf-8");

        pedido.Estado = "Completado";
        pedido.FechaEntrega = DateTime.UtcNow;
        pedido.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Content(PageHtml("¡Recepción confirmada!", "#d65391", "🎉", "Gracias <strong>" + pedido.NombreCliente + "</strong>, tu pedido ha sido marcado como completado.", "¡Esperamos que disfrutes tu compra. Vuelve pronto!"), "text/html; charset=utf-8");
    }

    [HttpPost("{id}/email-pago")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> EnviarEmailPago(int id, [FromBody] EnviarEmailPagoDto dto)
    {
        var pedido = await _db.Pedidos.FindAsync(id);
        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));
        if (string.IsNullOrEmpty(pedido.EmailCliente)) return BadRequest(ApiResponse<object>.Fail("El pedido no tiene email"));

        var banco = _config["BankAccount:Banco"] ?? "Banco XYZ";
        var cuenta = _config["BankAccount:NumeroCuenta"] ?? "1234567890";
        var titular = _config["BankAccount:Titular"] ?? "Selenne Boutique";
        var tipoCuenta = _config["BankAccount:TipoCuenta"] ?? "Ahorros";
        _ = _email.SendPendingPaymentEmailAsync(pedido.EmailCliente, pedido.NombreCliente, pedido.PedidoID, pedido.Total, dto.Mensaje ?? "", banco, cuenta, titular, tipoCuenta);

        if (pedido.ClienteID > 0)
            _ = _notif.CreateAsync(pedido.ClienteID, "💳 Información de pago",
                $"Te enviamos los datos bancarios para completar el pago de tu pedido #{id}. Revisa tu correo.", "warning", $"pedido-{id}");

        return Ok(ApiResponse<object>.Ok(new { }, "Correo de pago enviado"));
    }

    [HttpPost("{id}/email-guia")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> EnviarEmailGuia(int id, [FromForm] EnviarEmailGuiaDto dto)
    {
        var pedido = await _db.Pedidos.FindAsync(id);
        if (pedido == null) return NotFound(ApiResponse<object>.Fail("Pedido no encontrado"));
        if (string.IsNullOrEmpty(pedido.EmailCliente)) return BadRequest(ApiResponse<object>.Fail("El pedido no tiene email"));

        byte[]? fotoBytes = null;
        string? fotoMimeType = null;
        if (dto.Foto != null && dto.Foto.Length > 0)
        {
            using var ms = new MemoryStream();
            await dto.Foto.CopyToAsync(ms);
            fotoBytes = ms.ToArray();
            fotoMimeType = dto.Foto.ContentType ?? "image/jpeg";

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);
            var ext = Path.GetExtension(dto.Foto.FileName);
            var fileName = $"guia_{id}_{Guid.NewGuid():N}{ext}";
            await System.IO.File.WriteAllBytesAsync(Path.Combine(uploadsPath, fileName), fotoBytes);
        }

        pedido.NumeroGuia = dto.NumeroGuia;
        pedido.Transportadora = dto.Transportadora;
        pedido.FechaActualizacion = DateTime.UtcNow;

        var estadosTerminales = new[] { "Entregado", "Cancelado", "Rechazado", "Rechazada", "Completado", "Completada" };
        if (!estadosTerminales.Contains(pedido.Estado))
        {
            pedido.Estado = "Enviado";
            pedido.FechaEnvio = DateTime.UtcNow;
        }

        if (string.IsNullOrEmpty(pedido.ConfirmacionToken))
            pedido.ConfirmacionToken = Guid.NewGuid().ToString("N");

        await _db.SaveChangesAsync();

        var baseUrlConfirm = _config["AppSettings:BaseUrl"] ?? "http://localhost:5000";
        var confirmarUrl = $"{baseUrlConfirm}/api/pedidos/confirmar/{pedido.ConfirmacionToken}";

        _ = _email.SendShippingNotificationEmailAsync(pedido.EmailCliente, pedido.NombreCliente, pedido.PedidoID, dto.NumeroGuia, dto.Transportadora, fotoBytes, fotoMimeType, confirmarUrl);

        var guiaMensaje = string.IsNullOrEmpty(dto.NumeroGuia)
            ? $"Tu pedido ha sido despachado y está en camino."
            : $"Tu pedido fue despachado con guía {dto.NumeroGuia}" + (string.IsNullOrEmpty(dto.Transportadora) ? "." : $" por {dto.Transportadora}.");
        if (pedido.ClienteID > 0)
            _ = _notif.CreateAsync(pedido.ClienteID, "🚚 Pedido enviado", guiaMensaje, "info", $"pedido-{id}");

        return Ok(ApiResponse<object>.Ok(new { }, "Correo de guía enviado"));
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

        if (pedido.Estado == "Entregado" || pedido.Estado == "Completada" || pedido.Estado == "Completado")
            return BadRequest(ApiResponse<object>.Fail("No se puede cancelar un pedido entregado"));

        pedido.Estado = "Cancelado";
        pedido.FechaActualizacion = DateTime.UtcNow;
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
        ComprobantePago = p.ComprobantePago, Notas = p.Notas,
        Detalles = p.Detalles?.Select(d => new PedidoDetalleDto
        {
            ProductoID = d.ProductoID,
            ProductoNombre = d.Producto?.Nombre ?? "",
            ImagenProducto = d.ImagenProducto ?? d.Producto?.ImagenPrincipal,
            Talla = d.TallaNombre ?? d.Talla?.Nombre,
            Color = d.ColorNombre ?? d.Color?.Nombre,
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            Subtotal = d.Subtotal
        }).ToList() ?? new()
    };
}
