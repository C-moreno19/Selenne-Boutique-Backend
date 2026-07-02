using System.Text;
using System.Text.Json;

namespace SelenneApi.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;
    private static readonly HttpClient _http = new();

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    { _config = config; _logger = logger; }

    private static string Ref(int pedidoId) => $"SLN-{pedidoId + 10000}";

    private async Task SendAsync(string to, string subject, string body,
        byte[]? inlineImageBytes = null, string? inlineImageMime = null, string inlineCid = "paquete_selenne")
    {
        try
        {
            var apiKey = _config["Email:BrevoApiKey"] ?? "";
            var fromEmail = _config["Email:FromEmail"] ?? "noreply@selenne.com";
            var fromName = _config["Email:FromName"] ?? "Selenne Boutique";

            object payload;
            if (inlineImageBytes != null && inlineImageBytes.Length > 0)
            {
                payload = new
                {
                    sender = new { name = fromName, email = fromEmail },
                    to = new[] { new { email = to } },
                    subject,
                    htmlContent = body,
                    attachment = new[]
                    {
                        new
                        {
                            content = Convert.ToBase64String(inlineImageBytes),
                            name = "image.jpg",
                            contentType = inlineImageMime ?? "image/jpeg",
                            contentId = inlineCid
                        }
                    }
                };
            }
            else
            {
                payload = new
                {
                    sender = new { name = fromName, email = fromEmail },
                    to = new[] { new { email = to } },
                    subject,
                    htmlContent = body
                };
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
                _logger.LogInformation("Email enviado a {To}: {Subject}", to, subject);
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Email Brevo {Status} al enviar a {To}: {Err}", (int)response.StatusCode, to, err);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email error al enviar a {To}", to);
        }
    }

    private string Card(string content) =>
        "<html><body style='margin:0;padding:0;background:#fdf2f8;font-family:Arial,Helvetica,sans-serif'>" +
        "<table width='100%' cellpadding='0' cellspacing='0' style='padding:28px 16px'><tr><td>" +
        "<table width='100%' cellpadding='0' cellspacing='0' style='max-width:500px;margin:0 auto;background:#ffffff;box-shadow:0 2px 12px rgba(214,83,145,.08)'>" +
        "<tr><td style='background:#d65391;padding:24px 36px'>" +
        "<p style='margin:0;font-size:13px;font-weight:700;color:#fff;letter-spacing:3px;text-transform:uppercase'>Selenne Boutique</p>" +
        "</td></tr>" +
        "<tr><td style='padding:28px 36px 24px'>" + content + "</td></tr>" +
        "<tr><td style='background:#fdf2f8;padding:14px 36px;text-align:center;border-top:1px solid #fce7f3'>" +
        "<p style='margin:0;font-size:11px;color:#c084a5;letter-spacing:1px'>© Selenne Boutique — Moda con estilo</p>" +
        "</td></tr></table></td></tr></table></body></html>";

    private string Header(string titulo) =>
        "<h1 style='margin:0 0 16px 0;font-size:20px;color:#d65391;font-weight:700;letter-spacing:-0.3px;border-bottom:1px solid #fce7f3;padding-bottom:14px'>" + titulo + "</h1>";

    public async Task SendWelcomeEmailAsync(string to, string nombre) =>
        await SendAsync(to, "Bienvenida a Selenne Boutique",
            Card(Header("Cuenta creada") +
            "<p style='margin:0 0 16px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, gracias por unirte a Selenne Boutique. Tu cuenta está lista.</p>" +
            "<p style='margin:0;font-size:13px;color:#9ca3af'>Ahora puedes explorar nuestra colección y realizar pedidos.</p>"));

    public async Task SendVerificationEmailAsync(string to, string nombre, string token) =>
        await SendAsync(to, "Verifica tu correo — Selenne Boutique",
            Card(Header("Verificación de correo") +
            "<p style='margin:0 0 20px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, usa el siguiente código para verificar tu correo electrónico:</p>" +
            "<div style='background:#f9f9f9;border-left:3px solid #d65391;padding:14px 20px;margin:0 0 20px'>" +
            "<p style='margin:0;font-size:22px;font-weight:700;color:#d65391;letter-spacing:4px'>" + token + "</p>" +
            "</div>" +
            "<p style='margin:0;font-size:12px;color:#9ca3af'>Este código expira en 24 horas.</p>"));

    public async Task SendPasswordResetEmailAsync(string to, string nombre, string token) =>
        await SendAsync(to, "Restablecer contraseña — Selenne Boutique",
            Card(Header("Recuperar contraseña") +
            "<p style='margin:0 0 20px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, usa el siguiente código para restablecer tu contraseña:</p>" +
            "<div style='background:#f9f9f9;border-left:3px solid #d65391;padding:14px 20px;margin:0 0 20px'>" +
            "<p style='margin:0;font-size:22px;font-weight:700;color:#d65391;letter-spacing:4px'>" + token + "</p>" +
            "</div>" +
            "<p style='margin:0;font-size:12px;color:#9ca3af'>Este código expira en 1 hora. Si no solicitaste este cambio, ignora este mensaje.</p>"));

    public async Task SendPasswordChangedEmailAsync(string to, string nombre) =>
        await SendAsync(to, "Contraseña actualizada — Selenne Boutique",
            Card(Header("Contraseña actualizada") +
            "<p style='margin:0;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, tu contraseña fue actualizada exitosamente. Si no realizaste este cambio, contáctanos de inmediato.</p>"));

    public async Task SendNewUserCreatedEmailAsync(string to, string nombre, string tempPassword) =>
        await SendAsync(to, "Tu cuenta en Selenne Boutique",
            Card(Header("Cuenta creada") +
            "<p style='margin:0 0 20px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, tu cuenta fue creada en Selenne Boutique. Tu contraseña temporal es:</p>" +
            "<div style='background:#f9f9f9;border-left:3px solid #d65391;padding:14px 20px;margin:0 0 20px'>" +
            "<p style='margin:0;font-size:18px;font-weight:700;color:#111827;letter-spacing:1px'>" + tempPassword + "</p>" +
            "</div>" +
            "<p style='margin:0;font-size:12px;color:#9ca3af'>Cambia tu contraseña al ingresar por primera vez.</p>"));

    public async Task SendOrderConfirmationClienteAsync(string to, string nombre, int pedidoId, decimal total) =>
        await SendAsync(to, "Pedido recibido — Selenne Boutique",
            Card(Header("Pedido recibido") +
            "<p style='margin:0 0 20px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, hemos recibido tu pedido correctamente y pronto comenzaremos a prepararlo.</p>" +
            "<div style='background:#fafafa;border-left:3px solid #d65391;padding:16px 20px;margin:0 0 20px'>" +
            "<p style='margin:0;font-size:13px;color:#6b7280'>Total: <strong style='color:#111827'>$" + total.ToString("N0") + "</strong></p>" +
            "</div>" +
            "<p style='margin:0;font-size:12px;color:#9ca3af'>Te notificaremos cuando tu pedido sea procesado.</p>"));

    public async Task SendOrderConfirmationAdminAsync(string adminEmail, string clienteNombre, int pedidoId, decimal total) =>
        await SendAsync(adminEmail, "Nuevo pedido " + Ref(pedidoId) + " — Selenne Boutique",
            Card(Header("Nuevo pedido recibido") +
            "<div style='background:#fafafa;border-left:3px solid #d65391;padding:16px 20px;margin:0 0 20px'>" +
            "<p style='margin:0 0 6px;font-size:13px;color:#374151'>Referencia: <strong>" + Ref(pedidoId) + "</strong></p>" +
            "<p style='margin:0 0 6px;font-size:13px;color:#374151'>Cliente: <strong>" + clienteNombre + "</strong></p>" +
            "<p style='margin:0;font-size:13px;color:#374151'>Total: <strong>$" + total.ToString("N0") + "</strong></p>" +
            "</div>" +
            "<p style='margin:0;font-size:12px;color:#9ca3af'>Ingresa al panel de administración para gestionar el pedido.</p>"));

    public async Task SendOrderStatusUpdateAsync(string to, string nombre, int pedidoId, string nuevoEstado, string? motivo = null)
    {
        var esRechazo = nuevoEstado is "Rechazado" or "Rechazada";
        var motivoHtml = esRechazo && !string.IsNullOrWhiteSpace(motivo)
            ? "<div style='background:#fff5f5;border-left:3px solid #ef4444;padding:14px 20px;margin:12px 0 20px'>" +
              "<p style='margin:0 0 4px;font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.05em;color:#ef4444'>Motivo del rechazo</p>" +
              "<p style='margin:0;font-size:13px;color:#374151'>" + motivo + "</p></div>"
            : "";

        await SendAsync(to, nuevoEstado + " — Selenne Boutique",
            Card(Header("Estado del pedido actualizado") +
            "<p style='margin:0 0 16px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, el estado de tu pedido ha sido actualizado.</p>" +
            "<div style='background:#fafafa;border-left:3px solid #d65391;padding:16px 20px;margin:0 0 16px'>" +
            "<p style='margin:0;font-size:13px;color:#374151'>Nuevo estado: <strong>" + nuevoEstado + "</strong></p>" +
            "</div>" +
            motivoHtml +
            "<p style='margin:0;font-size:12px;color:#9ca3af'>Si tienes preguntas sobre tu pedido, contáctanos.</p>"));
    }

    public async Task SendPendingPaymentEmailAsync(string to, string nombre, int pedidoId, decimal total, string mensaje, string banco, string cuenta, string titular, string tipoCuenta)
    {
        var whatsapp = _config["BankAccount:WhatsApp"] ?? "";
        var waText = Uri.EscapeDataString($"Hola, adjunto el comprobante de pago de mi pedido por ${total:N0}");
        var waUrl = $"https://wa.me/{whatsapp}?text={waText}";

        var qrPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "qr-transferencia.png");
        byte[]? qrBytes = System.IO.File.Exists(qrPath) ? await System.IO.File.ReadAllBytesAsync(qrPath) : null;
        var qrImg = qrBytes != null
            ? "<img src='cid:qr_selenne' alt='QR transferencia' style='border:1px solid #e5e7eb;padding:4px;max-width:200px' />"
            : "";

        var waSection = string.IsNullOrEmpty(whatsapp) ? "" :
            "<div style='background:#fafafa;border:1px solid #f0f0f0;padding:16px 20px;margin:16px 0'>" +
            "<p style='margin:0 0 4px;font-size:11px;color:#9ca3af;text-transform:uppercase;letter-spacing:1.5px'>Comprobante por WhatsApp</p>" +
            "<p style='margin:0 0 12px;font-size:13px;color:#374151'>Una vez transferido, envía tu comprobante al siguiente número:</p>" +
            "<p style='margin:0 0 12px;font-size:18px;font-weight:700;color:#16a34a;letter-spacing:1px'>+" + whatsapp + "</p>" +
            "<a href='" + waUrl + "' style='display:inline-block;background:#25d366;color:white;padding:10px 24px;text-decoration:none;font-size:13px;font-weight:600'>Abrir WhatsApp</a>" +
            "</div>";

        var content =
            Header("Información de pago") +
            "<p style='margin:0 0 16px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, para completar tu pedido por <strong>$" + total.ToString("N0") + "</strong>, realiza la transferencia a:</p>" +
            (string.IsNullOrWhiteSpace(mensaje) ? "" : "<div style='background:#fef9c3;border-left:3px solid #eab308;padding:12px 16px;margin:0 0 16px'><p style='margin:0;font-size:13px;color:#374151'>" + mensaje + "</p></div>") +
            "<table style='width:100%;border-collapse:collapse;margin:0 0 16px'>" +
            "<tr><td style='padding:9px 12px;border:1px solid #e5e7eb;font-size:12px;color:#9ca3af;width:40%'>Banco</td><td style='padding:9px 12px;border:1px solid #e5e7eb;font-size:13px;font-weight:600'>" + banco + "</td></tr>" +
            "<tr><td style='padding:9px 12px;border:1px solid #e5e7eb;font-size:12px;color:#9ca3af'>Número de cuenta</td><td style='padding:9px 12px;border:1px solid #e5e7eb;font-size:15px;font-weight:700;color:#d65391'>" + cuenta + "</td></tr>" +
            "<tr><td style='padding:9px 12px;border:1px solid #e5e7eb;font-size:12px;color:#9ca3af'>Titular</td><td style='padding:9px 12px;border:1px solid #e5e7eb;font-size:13px;font-weight:600'>" + titular + "</td></tr>" +
            "<tr><td style='padding:9px 12px;border:1px solid #e5e7eb;font-size:12px;color:#9ca3af'>Tipo</td><td style='padding:9px 12px;border:1px solid #e5e7eb;font-size:13px;font-weight:600'>" + tipoCuenta + "</td></tr>" +
            "</table>" +
            (qrImg != "" ? "<div style='text-align:center;margin:0 0 16px'>" +
            "<p style='margin:0 0 8px;font-size:12px;color:#9ca3af'>Escanea el código QR con los datos bancarios:</p>" +
            qrImg + "</div>" : "") +
            waSection;
        await SendAsync(to, "Información de pago — Selenne Boutique", Card(content), qrBytes, "image/png", "qr_selenne");
    }

    public async Task SendShippingNotificationEmailAsync(string to, string nombre, int pedidoId, string? numeroGuia, string? transportadora, byte[]? fotoBytes = null, string? fotoMimeType = null, string? confirmarUrl = null)
    {
        var hasPhoto = fotoBytes != null && fotoBytes.Length > 0;
        var content =
            Header("Tu pedido está en camino") +
            "<p style='margin:0 0 16px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, tu pedido fue despachado y está en camino a tu dirección.</p>" +
            (string.IsNullOrEmpty(numeroGuia) ? "" :
                "<div style='background:#fafafa;border-left:3px solid #16a34a;padding:14px 20px;margin:0 0 16px'>" +
                "<p style='margin:0 0 4px;font-size:11px;color:#9ca3af;text-transform:uppercase;letter-spacing:1.5px'>Número de guía</p>" +
                "<p style='margin:0 0 4px;font-size:20px;font-weight:700;color:#16a34a;letter-spacing:2px'>" + numeroGuia + "</p>" +
                (string.IsNullOrEmpty(transportadora) ? "" : "<p style='margin:0;font-size:12px;color:#6b7280'>Transportadora: <strong>" + transportadora + "</strong></p>") +
                "</div>") +
            (hasPhoto ?
                "<div style='margin:0 0 16px'>" +
                "<p style='margin:0 0 8px;font-size:11px;color:#9ca3af;text-transform:uppercase;letter-spacing:1.5px'>Foto del paquete</p>" +
                "<img src='cid:paquete_selenne' alt='Paquete despachado' style='width:100%;max-width:100%;border:1px solid #e5e7eb;display:block' />" +
                "</div>" : "") +
            (string.IsNullOrEmpty(confirmarUrl) ? "" :
                "<div style='background:#fafafa;border:1px solid #f0f0f0;padding:20px;margin:0 0 4px;text-align:center'>" +
                "<p style='margin:0 0 4px;font-size:13px;color:#374151;font-weight:600'>¿Ya recibiste tu pedido?</p>" +
                "<p style='margin:0 0 16px;font-size:12px;color:#9ca3af'>Confirma la entrega cuando lo hayas recibido en casa.</p>" +
                "<a href='" + confirmarUrl + "' style='display:inline-block;background:#d65391;color:white;padding:12px 32px;text-decoration:none;font-size:14px;font-weight:600;letter-spacing:0.3px'>Confirmar recepción</a>" +
                "</div>");
        await SendAsync(to, "Tu pedido ha sido despachado — Selenne Boutique", Card(content),
            hasPhoto ? fotoBytes : null, fotoMimeType);
    }

    public async Task SendOrderApprovedAsync(string to, string nombre, int pedidoId, decimal total) =>
        await SendAsync(to, "Pedido aprobado — Selenne Boutique",
            Card(Header("Pedido aprobado") +
            "<p style='margin:0 0 16px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, tu pedido ha sido aprobado y será enviado en las próximas 72 horas.</p>" +
            "<div style='background:#fafafa;border-left:3px solid #d65391;padding:14px 20px;margin:0 0 20px'>" +
            "<p style='margin:0 0 4px;font-size:11px;color:#9ca3af;text-transform:uppercase;letter-spacing:1.5px'>Total</p>" +
            "<p style='margin:0;font-size:18px;font-weight:700;color:#111827'>$" + total.ToString("N0") + "</p>" +
            "</div>" +
            "<p style='margin:0;font-size:13px;color:#9ca3af'>Te notificaremos cuando tu pedido sea despachado con la información de envío.</p>"));
}
