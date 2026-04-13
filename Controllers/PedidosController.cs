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
            FechaActualizacion = DateTime.Now,
            ConfirmacionToken = Guid.NewGuid().ToString("N")
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
                    TallaNombre = item.TallaNombre,
                    ColorNombre = item.ColorNombre,
                    ImagenProducto = producto.ImagenPrincipal,
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

        // Notificar al cliente que el pedido fue recibido
        if (userId > 0)
            _ = _notif.CreateAsync(userId, "📦 Pedido recibido",
                $"Hemos recibido tu pedido #{pedido.PedidoID} por ${pedido.Total:N0}. Está pendiente de revisión.", "info", $"pedido-{pedido.PedidoID}");

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

        // Email al aprobar (respetar preferencia del cliente)
        if (dto.NuevoEstado == "Aprobado" && !string.IsNullOrEmpty(pedido.EmailCliente))
        {
            var token = pedido.ConfirmacionToken ?? Guid.NewGuid().ToString("N");
            if (pedido.ConfirmacionToken == null) { pedido.ConfirmacionToken = token; await _db.SaveChangesAsync(); }
            var cliente = pedido.ClienteID > 0 ? await _db.Usuarios.FindAsync(pedido.ClienteID) : null;
            if (cliente == null || cliente.NotificacionesEmail)
            {
                var baseUrl = _config["AppSettings:BaseUrl"] ?? "http://localhost:5000";
                var confirmarUrl = $"{baseUrl}/api/pedidos/confirmar/{token}";
                _ = _email.SendOrderApprovedAsync(pedido.EmailCliente, pedido.NombreCliente, pedido.PedidoID, pedido.Total, confirmarUrl);
            }
        }

        // Notificación en app según el nuevo estado
        var cid = pedido.ClienteID;
        if (cid > 0)
        {
            (string titulo, string mensaje, string tipo) notif = dto.NuevoEstado switch
            {
                "Aprobado"   => ("✅ Pedido aprobado",   $"Tu pedido #{pedido.PedidoID} fue aprobado y será enviado en las próximas 72 horas.", "success"),
                "Rechazado"  => ("❌ Pedido rechazado",  $"Tu pedido #{pedido.PedidoID} fue rechazado." + (string.IsNullOrEmpty(dto.Notas) ? "" : $" Motivo: {dto.Notas}"), "error"),
                "Completado" => ("🎉 Pedido completado", $"Tu pedido #{pedido.PedidoID} fue completado. ¡Gracias por tu compra!", "success"),
                "Cancelado"  => ("Pedido cancelado",     $"Tu pedido #{pedido.PedidoID} fue cancelado.", "warning"),
                _ => ("", "", "")
            };
            if (!string.IsNullOrEmpty(notif.titulo))
                _ = _notif.CreateAsync(cid, notif.titulo, notif.mensaje, notif.tipo, $"pedido-{pedido.PedidoID}");
        }

        return Ok(ApiResponse<object>.Ok(new { estado = dto.NuevoEstado }, "Estado actualizado"));
    }

    [HttpGet("confirmar/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmarEntrega(string token)
    {
        string PageHtml(string titulo, string tituloColor, string icono, string mensaje, string subMensaje = "") =>
            "<!DOCTYPE html><html lang='es'><head><meta charset='UTF-8'><meta name='viewport' content='width=device-width,initial-scale=1'><title>Selenne Boutique</title>" +
            "<style>*{margin:0;padding:0;box-sizing:border-box}body{font-family:'Segoe UI',Arial,sans-serif;background:linear-gradient(135deg,#fce7f3,#fff7fb);min-height:100vh;display:flex;align-items:center;justify-content:center;padding:20px}" +
            ".card{background:white;border-radius:20px;box-shadow:0 20px 60px rgba(214,83,145,.15);max-width:480px;width:100%;overflow:hidden;text-align:center}" +
            ".header{background:linear-gradient(135deg,#d65391,#f8a9c5);padding:40px 30px}" +
            ".header h1{color:white;font-size:26px;font-weight:700;letter-spacing:1px}" +
            ".header p{color:rgba(255,255,255,.85);font-size:13px;margin-top:6px}" +
            ".body{padding:40px 30px}.icon{font-size:56px;margin-bottom:16px}" +
            ".titulo{font-size:22px;font-weight:700;color:" + tituloColor + ";margin-bottom:12px}" +
            ".mensaje{color:#374151;font-size:15px;line-height:1.6;margin-bottom:8px}" +
            ".sub{color:#9ca3af;font-size:13px;margin-top:12px}" +
            ".footer{background:#fdf2f8;padding:16px;border-top:1px solid #fce7f3}" +
            ".footer p{color:#d65391;font-size:12px;font-weight:600}</style></head>" +
            "<body><div class='card'><div class='header'><h1>Selenne Boutique</h1><p>Moda con estilo</p></div>" +
            "<div class='body'><div class='icon'>" + icono + "</div>" +
            "<p class='titulo'>" + titulo + "</p><p class='mensaje'>" + mensaje + "</p>" +
            (subMensaje != "" ? "<p class='sub'>" + subMensaje + "</p>" : "") +
            "</div><div class='footer'><p>selenneboutique.com</p></div></div></body></html>";

        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.ConfirmacionToken == token);
        if (pedido == null)
            return Content(PageHtml("Enlace no válido", "#dc2626", "❌", "Este enlace es inválido o ya fue utilizado anteriormente.", "Si crees que es un error, contacta a Selenne Boutique."), "text/html; charset=utf-8");

        if (pedido.Estado == "Completado")
            return Content(PageHtml("Pedido ya confirmado", "#16a34a", "✅", "Tu pedido <strong>#" + pedido.PedidoID + "</strong> ya fue marcado como completado. ¡Gracias por tu compra!", "Esperamos verte pronto de nuevo."), "text/html; charset=utf-8");

        pedido.Estado = "Completado";
        pedido.FechaEntrega = DateTime.Now;
        pedido.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return Content(PageHtml("¡Recepción confirmada!", "#16a34a", "🎉", "Gracias <strong>" + pedido.NombreCliente + "</strong>, tu pedido <strong>#" + pedido.PedidoID + "</strong> ha sido marcado como completado.", "¡Esperamos que disfrutes tu compra. Vuelve pronto!"), "text/html; charset=utf-8");
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

        string? fotoUrl = null;
        if (dto.Foto != null && dto.Foto.Length > 0)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);
            var ext = Path.GetExtension(dto.Foto.FileName);
            var fileName = $"guia_{id}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                await dto.Foto.CopyToAsync(stream);
            var baseUrl = _config["AppSettings:BaseUrl"] ?? "http://localhost:5000";
            fotoUrl = $"{baseUrl}/uploads/{fileName}";
        }

        pedido.NumeroGuia = dto.NumeroGuia;
        pedido.Transportadora = dto.Transportadora;
        pedido.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        _ = _email.SendShippingNotificationEmailAsync(pedido.EmailCliente, pedido.NombreCliente, pedido.PedidoID, dto.NumeroGuia, dto.Transportadora, fotoUrl);

        var guiaMensaje = string.IsNullOrEmpty(dto.NumeroGuia)
            ? $"Tu pedido #{id} ha sido despachado y está en camino."
            : $"Tu pedido #{id} fue despachado con guía {dto.NumeroGuia}" + (string.IsNullOrEmpty(dto.Transportadora) ? "." : $" por {dto.Transportadora}.");
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
            ImagenProducto = d.ImagenProducto ?? d.Producto?.ImagenPrincipal,
            Talla = d.TallaNombre ?? d.Talla?.Nombre,
            Color = d.ColorNombre ?? d.Color?.Nombre,
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            Subtotal = d.Subtotal
        }).ToList() ?? new()
    };
}