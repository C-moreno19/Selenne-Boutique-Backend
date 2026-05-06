using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SelenneApi.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    { _config = config; _logger = logger; }

    // Genera un código de referencia que no expone el volumen de ventas al cliente
    private static string Ref(int pedidoId) => $"SLN-{pedidoId + 10000}";

    private async Task SendAsync(string to, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["Email:FromName"] ?? "Selenne Boutique",
                _config["Email:FromEmail"] ?? "noreply@selenne.com"
            ));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["Email:SmtpHost"] ?? "smtp.gmail.com",
                int.Parse(_config["Email:SmtpPort"] ?? "587"),
                SecureSocketOptions.StartTls
            );
            await client.AuthenticateAsync(
                _config["Email:SmtpUsername"],
                _config["Email:SmtpPassword"]
            );
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email enviado a {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email error al enviar a {To}", to);
        }
    }

    private string Wrap(string t, string b) =>
        "<html><body style='font-family:Arial'>" +
        "<h1 style='color:#e91e8c'>Selenne Boutique</h1><h2>" + t + "</h2>" + b +
        "</body></html>";

    public async Task SendWelcomeEmailAsync(string to, string nombre) =>
        await SendAsync(to, "Bienvenida a Selenne", Wrap("Bienvenida!", "<p>Hola " + nombre + ", gracias por registrarte en Selenne Boutique.</p>"));

    public async Task SendVerificationEmailAsync(string to, string nombre, string token) =>
        await SendAsync(to, "Verifica tu email", Wrap("Verificacion", "<p>Hola " + nombre + ", tu token de verificacion es: <b>" + token + "</b>. Expira en 24h.</p>"));

    public async Task SendPasswordResetEmailAsync(string to, string nombre, string token) =>
        await SendAsync(to, "Restablecer contrasena", Wrap("Recuperar contrasena", "<p>Hola " + nombre + ", tu token para restablecer la contrasena es: <b>" + token + "</b>. Expira en 1h.</p>"));

    public async Task SendPasswordChangedEmailAsync(string to, string nombre) =>
        await SendAsync(to, "Contrasena actualizada", Wrap("Contrasena cambiada", "<p>Hola " + nombre + ", tu contrasena fue actualizada exitosamente.</p>"));

    public async Task SendNewUserCreatedEmailAsync(string to, string nombre, string tempPassword) =>
        await SendAsync(to, "Tu cuenta en Selenne", Wrap("Cuenta creada", "<p>Hola " + nombre + ", tu contrasena temporal es: <b>" + tempPassword + "</b>. Cambiala al ingresar.</p>"));

    public async Task SendOrderConfirmationClienteAsync(string to, string nombre, int pedidoId, decimal total) =>
        await SendAsync(to, "Pedido " + Ref(pedidoId) + " confirmado", Wrap("Pedido confirmado",
            "<p>Hola " + nombre + ", tu pedido <strong>" + Ref(pedidoId) + "</strong> fue recibido exitosamente.</p>" +
            "<p>Total: <b>$" + total.ToString("N2") + "</b></p>"));

    public async Task SendOrderConfirmationAdminAsync(string adminEmail, string clienteNombre, int pedidoId, decimal total) =>
        await SendAsync(adminEmail, "Nuevo pedido #" + pedidoId, Wrap("Nuevo pedido",
            "<p>Cliente: <b>" + clienteNombre + "</b></p>" +
            "<p>Total: <b>$" + total.ToString("N2") + "</b></p>"));

    public async Task SendOrderStatusUpdateAsync(string to, string nombre, int pedidoId, string nuevoEstado) =>
        await SendAsync(to, "Pedido " + Ref(pedidoId) + ": " + nuevoEstado, Wrap("Estado actualizado",
            "<p>Hola " + nombre + ", tu pedido <strong>" + Ref(pedidoId) + "</strong> ahora esta en estado: <b>" + nuevoEstado + "</b></p>"));

    public async Task SendPendingPaymentEmailAsync(string to, string nombre, int pedidoId, decimal total, string mensaje, string banco, string cuenta, string titular, string tipoCuenta)
    {
        var whatsapp = _config["BankAccount:WhatsApp"] ?? "";
        var accountInfo = $"Banco: {banco} | Cuenta: {cuenta} | Titular: {titular}";
        var qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?data={Uri.EscapeDataString(accountInfo)}&size=200x200&bgcolor=ffffff";
        var waText = Uri.EscapeDataString($"Hola, adjunto el comprobante de pago del pedido {Ref(pedidoId)} por ${total:N0}");
        var waUrl = $"https://wa.me/{whatsapp}?text={waText}";

        var waSection = string.IsNullOrEmpty(whatsapp) ? "" :
            "<div style='background:#f0fdf4;border:1px solid #bbf7d0;border-radius:12px;padding:20px;margin:24px 0;text-align:center'>" +
            "<p style='margin:0 0 6px 0;font-size:14px;color:#374151;font-weight:600'>📲 Envía tu comprobante por WhatsApp</p>" +
            "<p style='margin:0 0 16px 0;font-size:13px;color:#6b7280'>Una vez realizada la transferencia, envíanos la foto del comprobante al siguiente número:</p>" +
            "<p style='margin:0 0 16px 0;font-size:22px;font-weight:bold;color:#16a34a;letter-spacing:1px'>+" + whatsapp + "</p>" +
            "<a href='" + waUrl + "' style='display:inline-block;background:#25d366;color:white;padding:12px 28px;border-radius:8px;text-decoration:none;font-size:15px;font-weight:bold'>" +
            "💬 Abrir WhatsApp" +
            "</a>" +
            "<p style='margin:12px 0 0 0;font-size:11px;color:#9ca3af'>Indica en el mensaje tu número de referencia: <strong>" + Ref(pedidoId) + "</strong></p>" +
            "</div>";

        var body =
            "<html><body style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px'>" +
            "<div style='background:linear-gradient(135deg,#d65391,#f8a9c5);padding:30px;text-align:center;border-radius:12px 12px 0 0'>" +
            "<h1 style='color:white;margin:0;font-size:28px'>Selenne Boutique</h1></div>" +
            "<div style='background:white;padding:30px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 12px 12px'>" +
            "<h2 style='color:#d65391'>💳 Información de Pago — Pedido " + Ref(pedidoId) + "</h2>" +
            "<p>Hola <strong>" + nombre + "</strong>,</p>" +
            (string.IsNullOrWhiteSpace(mensaje) ? "" : "<p style='background:#fef9c3;border-left:4px solid #eab308;padding:12px 16px;border-radius:6px'>" + mensaje + "</p>") +
            "<p>Para completar tu pedido por <strong>$" + total.ToString("N0") + "</strong>, realiza la transferencia a la siguiente cuenta:</p>" +
            "<table style='width:100%;border-collapse:collapse;margin:20px 0'>" +
            "<tr style='background:#f9fafb'><td style='padding:10px 14px;border:1px solid #e5e7eb;font-weight:bold;color:#6b7280;font-size:13px'>Banco</td><td style='padding:10px 14px;border:1px solid #e5e7eb;font-weight:600'>" + banco + "</td></tr>" +
            "<tr><td style='padding:10px 14px;border:1px solid #e5e7eb;font-weight:bold;color:#6b7280;font-size:13px'>Número de cuenta</td><td style='padding:10px 14px;border:1px solid #e5e7eb;font-weight:600;font-size:16px;color:#d65391'>" + cuenta + "</td></tr>" +
            "<tr style='background:#f9fafb'><td style='padding:10px 14px;border:1px solid #e5e7eb;font-weight:bold;color:#6b7280;font-size:13px'>Titular</td><td style='padding:10px 14px;border:1px solid #e5e7eb;font-weight:600'>" + titular + "</td></tr>" +
            "<tr><td style='padding:10px 14px;border:1px solid #e5e7eb;font-weight:bold;color:#6b7280;font-size:13px'>Tipo de cuenta</td><td style='padding:10px 14px;border:1px solid #e5e7eb;font-weight:600'>" + tipoCuenta + "</td></tr>" +
            "</table>" +
            "<div style='text-align:center;margin:24px 0'>" +
            "<p style='color:#6b7280;margin-bottom:10px'>Escanea el código QR para guardar los datos bancarios:</p>" +
            "<img src='" + qrUrl + "' alt='QR pago' style='border:1px solid #e5e7eb;border-radius:8px;padding:6px' />" +
            "</div>" +
            waSection +
            "<hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'>" +
            "<p style='font-size:12px;color:#9ca3af;text-align:center'>Selenne Boutique — Moda con estilo</p>" +
            "</div></body></html>";
        await SendAsync(to, "Pedido " + Ref(pedidoId) + " — Información de pago", body);
    }

    public async Task SendShippingNotificationEmailAsync(string to, string nombre, int pedidoId, string? numeroGuia, string? transportadora, string? fotoUrl)
    {
        var body =
            "<html><body style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px'>" +
            "<div style='background:linear-gradient(135deg,#d65391,#f8a9c5);padding:30px;text-align:center;border-radius:12px 12px 0 0'>" +
            "<h1 style='color:white;margin:0;font-size:28px'>Selenne Boutique</h1></div>" +
            "<div style='background:white;padding:30px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 12px 12px'>" +
            "<h2 style='color:#16a34a'>🚚 ¡Tu pedido está en camino!</h2>" +
            "<p>Hola <strong>" + nombre + "</strong>,</p>" +
            "<p>Tu pedido <strong>" + Ref(pedidoId) + "</strong> ha sido despachado y está en camino a tu dirección.</p>" +
            (string.IsNullOrEmpty(numeroGuia) ? "" :
                "<div style='background:#f0fdf4;border:1px solid #bbf7d0;border-radius:8px;padding:16px;margin:20px 0'>" +
                "<p style='margin:0 0 6px 0;color:#6b7280;font-size:13px;font-weight:bold'>NÚMERO DE GUÍA</p>" +
                "<p style='margin:0;font-size:22px;font-weight:bold;color:#16a34a;letter-spacing:1px'>" + numeroGuia + "</p>" +
                (string.IsNullOrEmpty(transportadora) ? "" : "<p style='margin:6px 0 0 0;color:#374151;font-size:14px'>Transportadora: <strong>" + transportadora + "</strong></p>") +
                "</div>") +
            (string.IsNullOrEmpty(fotoUrl) ? "" :
                "<div style='text-align:center;margin:20px 0'>" +
                "<p style='color:#6b7280;margin-bottom:10px;font-size:14px'>📸 Foto del paquete despachado:</p>" +
                "<img src='" + fotoUrl + "' alt='Paquete' style='max-width:100%;max-height:300px;border-radius:10px;border:1px solid #e5e7eb' />" +
                "</div>") +
            "<p style='font-size:14px;color:#374151'>Pronto recibirás tu pedido. ¡Gracias por comprar en Selenne Boutique!</p>" +
            "<hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'>" +
            "<p style='font-size:12px;color:#9ca3af;text-align:center'>Selenne Boutique — Moda con estilo</p>" +
            "</div></body></html>";
        await SendAsync(to, "🚚 Tu pedido " + Ref(pedidoId) + " fue despachado - Selenne Boutique", body);
    }

    public async Task SendOrderApprovedAsync(string to, string nombre, int pedidoId, decimal total, string confirmarUrl) =>
        await SendAsync(to, "✅ Tu pedido " + Ref(pedidoId) + " fue aprobado - Selenne Boutique",
            "<html><body style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px'>" +
            "<div style='background:linear-gradient(135deg,#d65391,#f8a9c5);padding:30px;text-align:center;border-radius:12px 12px 0 0'>" +
            "<h1 style='color:white;margin:0;font-size:28px'>Selenne Boutique</h1></div>" +
            "<div style='background:white;padding:30px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 12px 12px'>" +
            "<h2 style='color:#16a34a'>✅ ¡Tu pedido fue aprobado!</h2>" +
            "<p>Hola <strong>" + nombre + "</strong>,</p>" +
            "<p>Tu pedido <strong>" + Ref(pedidoId) + "</strong> ha sido aprobado y será enviado en las próximas <strong>72 horas</strong>.</p>" +
            "<p style='color:#6b7280'>Total: <strong>$" + total.ToString("N0") + "</strong></p>" +
            "<p>Cuando recibas tu pedido, confirma la entrega haciendo clic en el siguiente botón:</p>" +
            "<div style='text-align:center;margin:36px 0'>" +
            "<a href='" + confirmarUrl + "' style='background:linear-gradient(135deg,#d65391,#b8306e);color:white;padding:18px 44px;border-radius:50px;text-decoration:none;font-size:17px;font-weight:700;display:inline-block;box-shadow:0 6px 20px rgba(214,83,145,0.45);letter-spacing:0.3px'>📦 Ya recibí mi pedido</a>" +
            "</div>" +
            "<p style='font-size:13px;color:#9ca3af;text-align:center;margin-top:-10px'>Toca el botón cuando hayas recibido tu pedido en casa</p>" +
            "<p style='font-size:12px;color:#9ca3af'>Si no puedes hacer clic en el botón, copia y pega este enlace en tu navegador:<br>" +
            "<a href='" + confirmarUrl + "' style='color:#d65391'>" + confirmarUrl + "</a></p>" +
            "<hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'>" +
            "<p style='font-size:12px;color:#9ca3af;text-align:center'>Selenne Boutique — Moda con estilo</p>" +
            "</div></body></html>");
}