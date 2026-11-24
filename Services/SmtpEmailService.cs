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

    private static string Ref(int pedidoId) => $"DS-{pedidoId + 10000}";

    private async Task SendAsync(string to, string subject, string body,
        byte[]? inlineImageBytes = null, string? inlineImageMime = null, string inlineCid = "paquete_district")
    {
        try
        {
            var apiKey = _config["Email:BrevoApiKey"] ?? "";
            var fromEmail = _config["Email:FromEmail"] ?? "noreply@districtstore.com";
            var fromName = _config["Email:FromName"] ?? "District Store";

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

    // Paleta de marca District Store: negro -> morado oscuro, sobre fondo claro
    // neutro (no blanco puro, para que el header con degradado no quede flotando).
    private const string Gradient = "background:linear-gradient(90deg,#0A0A0A 0%,#3B0A45 55%,#5C2A52 100%)";

    private string Card(string content, string? eyebrow = null) =>
        "<html><body style='margin:0;padding:0;background:#F5F3FA;font-family:Arial,Helvetica,sans-serif'>" +
        "<table width='100%' cellpadding='0' cellspacing='0' style='padding:32px 16px'><tr><td>" +
        "<table width='100%' cellpadding='0' cellspacing='0' style='max-width:520px;margin:0 auto;background:#ffffff;border:1px solid #E7E0DA'>" +
        "<tr><td style='" + Gradient + ";padding:28px 36px'>" +
        "<p style='margin:0 0 6px;font-size:11px;font-weight:700;color:#C9A8C3;letter-spacing:4px;text-transform:uppercase'>" + (eyebrow ?? "District Store") + "</p>" +
        "<p style='margin:0;font-size:22px;font-weight:700;color:#ffffff;font-family:Georgia,\"Times New Roman\",serif;letter-spacing:0.3px'>District Store</p>" +
        "</td></tr>" +
        "<tr><td style='padding:32px 36px 28px'>" + content + "</td></tr>" +
        "<tr><td style='background:#F5F3FA;padding:18px 36px;text-align:center;border-top:1px solid #E7E0DA'>" +
        "<p style='margin:0 0 4px;font-size:11px;color:#8a7a82;letter-spacing:1px'>© District Store — Streetwear y esenciales para hombre y mujer</p>" +
        "<p style='margin:0;font-size:11px;color:#b9aedd'>Hecho para la calle, entregado en tu puerta.</p>" +
        "</td></tr></table></td></tr></table></body></html>";

    private string Header(string titulo) =>
        "<h1 style='margin:0 0 18px 0;font-size:22px;color:#241B22;font-weight:700;font-family:Georgia,\"Times New Roman\",serif;letter-spacing:-0.2px;border-bottom:2px solid #5C2A52;padding-bottom:14px'>" + titulo + "</h1>";

    private const string AccentBox = "background:#F7F5FC;border-left:3px solid #5C2A52;padding:16px 20px;margin:0 0 20px";

    public async Task SendWelcomeEmailAsync(string to, string nombre) =>
        await SendAsync(to, "¡Bienvenido a District Store! 🖤",
            Card(Header("Ya eres parte del movimiento") +
            "<p style='margin:0 0 16px;font-size:14px;color:#374151;line-height:1.6'>¿Qué más, <strong>" + nombre + "</strong>? Tu cuenta ya está lista. Bienvenido a District Store — streetwear con actitud, hecho para quienes marcan su propio camino.</p>" +
            "<p style='margin:0;font-size:13px;color:#7d6f77'>Explora la colección, arma tu fit y hazlo tuyo. Vamos con todo. 🔥</p>"));

    public async Task SendVerificationEmailAsync(string to, string nombre, string token) =>
        await SendAsync(to, "Verifica tu correo — District Store",
            Card(Header("Un paso más") +
            "<p style='margin:0 0 20px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, usa este código para verificar tu correo y quedar listo para comprar:</p>" +
            "<div style='" + AccentBox + "text-align:center'>" +
            "<p style='margin:0;font-size:26px;font-weight:700;color:#5C2A52;letter-spacing:6px'>" + token + "</p>" +
            "</div>" +
            "<p style='margin:0;font-size:12px;color:#a89ca2'>Este código expira en 24 horas.</p>"));

    public async Task SendPasswordResetEmailAsync(string to, string nombre, string token) =>
        await SendAsync(to, "Restablecer contraseña — District Store",
            Card(Header("Recupera tu acceso") +
            "<p style='margin:0 0 20px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, usa este código para crear una contraseña nueva:</p>" +
            "<div style='" + AccentBox + "text-align:center'>" +
            "<p style='margin:0;font-size:26px;font-weight:700;color:#5C2A52;letter-spacing:6px'>" + token + "</p>" +
            "</div>" +
            "<p style='margin:0;font-size:12px;color:#a89ca2'>Este código expira en 1 hora. Si no solicitaste este cambio, ignora este mensaje.</p>"));

    public async Task SendPasswordChangedEmailAsync(string to, string nombre) =>
        await SendAsync(to, "Contraseña actualizada — District Store",
            Card(Header("Todo en orden") +
            "<p style='margin:0;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, tu contraseña fue actualizada con éxito. Si no fuiste tú, escríbenos de inmediato y lo resolvemos.</p>"));

    public async Task SendNewUserCreatedEmailAsync(string to, string nombre, string tempPassword) =>
        await SendAsync(to, "Tu cuenta en District Store está lista",
            Card(Header("Bienvenido al equipo") +
            "<p style='margin:0 0 20px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, te creamos una cuenta en District Store. Tu contraseña temporal es:</p>" +
            "<div style='" + AccentBox + "text-align:center'>" +
            "<p style='margin:0;font-size:19px;font-weight:700;color:#241B22;letter-spacing:1px'>" + tempPassword + "</p>" +
            "</div>" +
            "<p style='margin:0;font-size:12px;color:#a89ca2'>Cámbiala apenas inicies sesión por primera vez.</p>"));

    public async Task SendOrderConfirmationClienteAsync(string to, string nombre, int pedidoId, decimal total) =>
        await SendAsync(to, "¡Pedido recibido! — District Store",
            Card(Header("Tu pedido ya está con nosotros") +
            "<p style='margin:0 0 20px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, recibimos tu pedido y ya estamos preparándolo con todo el cuidado. Pronto lo tendrás puesto.</p>" +
            "<div style='" + AccentBox + "'>" +
            "<p style='margin:0 0 4px;font-size:11px;color:#7d6f77;text-transform:uppercase;letter-spacing:1.5px'>Referencia</p>" +
            "<p style='margin:0 0 10px;font-size:14px;font-weight:600;color:#241B22'>" + Ref(pedidoId) + "</p>" +
            "<p style='margin:0;font-size:13px;color:#7d6f77'>Total: <strong style='color:#241B22'>$" + total.ToString("N0") + "</strong></p>" +
            "</div>" +
            "<p style='margin:0;font-size:12px;color:#a89ca2'>Te avisamos apenas tu pedido avance de estado.</p>"));

    public async Task SendOrderConfirmationAdminAsync(string adminEmail, string clienteNombre, int pedidoId, decimal total) =>
        await SendAsync(adminEmail, "Nuevo pedido " + Ref(pedidoId) + " — District Store",
            Card(Header("Nuevo pedido recibido") +
            "<div style='" + AccentBox + "'>" +
            "<p style='margin:0 0 6px;font-size:13px;color:#374151'>Referencia: <strong>" + Ref(pedidoId) + "</strong></p>" +
            "<p style='margin:0 0 6px;font-size:13px;color:#374151'>Cliente: <strong>" + clienteNombre + "</strong></p>" +
            "<p style='margin:0;font-size:13px;color:#374151'>Total: <strong>$" + total.ToString("N0") + "</strong></p>" +
            "</div>" +
            "<p style='margin:0;font-size:12px;color:#a89ca2'>Ingresa al panel de administración para gestionar el pedido.</p>"));

    public async Task SendOrderStatusUpdateAsync(string to, string nombre, int pedidoId, string nuevoEstado, string? motivo = null)
    {
        var esRechazo = nuevoEstado is "Rechazado" or "Rechazada";
        var motivoHtml = esRechazo && !string.IsNullOrWhiteSpace(motivo)
            ? "<div style='background:#fff5f5;border-left:3px solid #ef4444;padding:14px 20px;margin:12px 0 20px'>" +
              "<p style='margin:0 0 4px;font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.05em;color:#ef4444'>Motivo del rechazo</p>" +
              "<p style='margin:0;font-size:13px;color:#374151'>" + motivo + "</p></div>"
            : "";

        await SendAsync(to, nuevoEstado + " — District Store",
            Card(Header("Actualización de tu pedido") +
            "<p style='margin:0 0 16px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, el estado de tu pedido cambió.</p>" +
            "<div style='" + AccentBox + "'>" +
            "<p style='margin:0;font-size:13px;color:#374151'>Nuevo estado: <strong style='color:#241B22'>" + nuevoEstado + "</strong></p>" +
            "</div>" +
            motivoHtml +
            "<p style='margin:0;font-size:12px;color:#a89ca2'>¿Dudas sobre tu pedido? Aquí estamos para ayudarte.</p>"));
    }

    public async Task SendPendingPaymentEmailAsync(string to, string nombre, int pedidoId, decimal total, string mensaje, string banco, string cuenta, string titular, string tipoCuenta)
    {
        var whatsapp = _config["BankAccount:WhatsApp"] ?? "";
        var waText = Uri.EscapeDataString($"Hola, adjunto el comprobante de pago de mi pedido por ${total:N0}");
        var waUrl = $"https://wa.me/{whatsapp}?text={waText}";

        var qrPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "qr-transferencia.png");
        byte[]? qrBytes = System.IO.File.Exists(qrPath) ? await System.IO.File.ReadAllBytesAsync(qrPath) : null;
        var qrImg = qrBytes != null
            ? "<img src='cid:qr_district' alt='Código QR para transferencia' style='border:1px solid #E7E0DA;padding:4px;max-width:200px' />"
            : "";

        var waSection = string.IsNullOrEmpty(whatsapp) ? "" :
            "<div style='background:#F7F5FC;border:1px solid #E7E0DA;padding:16px 20px;margin:16px 0'>" +
            "<p style='margin:0 0 4px;font-size:11px;color:#a89ca2;text-transform:uppercase;letter-spacing:1.5px'>Comprobante por WhatsApp</p>" +
            "<p style='margin:0 0 12px;font-size:13px;color:#374151'>Una vez transferido, envíanos tu comprobante a este número:</p>" +
            "<p style='margin:0 0 12px;font-size:18px;font-weight:700;color:#16a34a;letter-spacing:1px'>+" + whatsapp + "</p>" +
            "<a href='" + waUrl + "' style='display:inline-block;background:#25d366;color:white;padding:10px 24px;text-decoration:none;font-size:13px;font-weight:600'>Abrir WhatsApp</a>" +
            "</div>";

        var content =
            Header("Falta solo un paso") +
            "<p style='margin:0 0 16px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, para asegurar tu pedido por <strong>$" + total.ToString("N0") + "</strong>, transfiere a:</p>" +
            (string.IsNullOrWhiteSpace(mensaje) ? "" : "<div style='background:#fef9c3;border-left:3px solid #eab308;padding:12px 16px;margin:0 0 16px'><p style='margin:0;font-size:13px;color:#374151'>" + mensaje + "</p></div>") +
            "<table style='width:100%;border-collapse:collapse;margin:0 0 16px'>" +
            "<tr><td style='padding:9px 12px;border:1px solid #E7E0DA;font-size:12px;color:#a89ca2;width:40%'>Banco</td><td style='padding:9px 12px;border:1px solid #E7E0DA;font-size:13px;font-weight:600'>" + banco + "</td></tr>" +
            "<tr><td style='padding:9px 12px;border:1px solid #E7E0DA;font-size:12px;color:#a89ca2'>Llave Bre-B</td><td style='padding:9px 12px;border:1px solid #E7E0DA;font-size:15px;font-weight:700;color:#5C2A52'>" + cuenta + "<br/><span style='font-size:11px;font-weight:400;color:#a89ca2'>Pégala en la opción \"Bre-B\" / \"Llave\" de tu app bancaria, no es un número de celular</span></td></tr>" +
            "<tr><td style='padding:9px 12px;border:1px solid #E7E0DA;font-size:12px;color:#a89ca2'>Titular</td><td style='padding:9px 12px;border:1px solid #E7E0DA;font-size:13px;font-weight:600'>" + titular + "</td></tr>" +
            "<tr><td style='padding:9px 12px;border:1px solid #E7E0DA;font-size:12px;color:#a89ca2'>Tipo</td><td style='padding:9px 12px;border:1px solid #E7E0DA;font-size:13px;font-weight:600'>" + tipoCuenta + "</td></tr>" +
            "</table>" +
            (qrImg != "" ? "<div style='text-align:center;margin:0 0 16px'>" +
            "<p style='margin:0 0 8px;font-size:12px;color:#a89ca2'>O escanea el código QR con los datos bancarios:</p>" +
            qrImg + "</div>" : "") +
            waSection;
        await SendAsync(to, "Información de pago — District Store", Card(content), qrBytes, "image/png", "qr_district");
    }

    public async Task SendShippingNotificationEmailAsync(string to, string nombre, int pedidoId, string? numeroGuia, string? transportadora, byte[]? fotoBytes = null, string? fotoMimeType = null, string? confirmarUrl = null)
    {
        var hasPhoto = fotoBytes != null && fotoBytes.Length > 0;
        var content =
            Header("¡Ya va en camino! 🚚") +
            "<p style='margin:0 0 16px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, tu pedido salió de nuestras manos y va directo a las tuyas.</p>" +
            (string.IsNullOrEmpty(numeroGuia) ? "" :
                "<div style='background:#F7F5FC;border-left:3px solid #16a34a;padding:14px 20px;margin:0 0 16px'>" +
                "<p style='margin:0 0 4px;font-size:11px;color:#a89ca2;text-transform:uppercase;letter-spacing:1.5px'>Número de guía</p>" +
                "<p style='margin:0 0 4px;font-size:20px;font-weight:700;color:#16a34a;letter-spacing:2px'>" + numeroGuia + "</p>" +
                (string.IsNullOrEmpty(transportadora) ? "" : "<p style='margin:0;font-size:12px;color:#7d6f77'>Transportadora: <strong>" + transportadora + "</strong></p>") +
                "</div>") +
            (hasPhoto ?
                "<div style='margin:0 0 16px'>" +
                "<p style='margin:0 0 8px;font-size:11px;color:#a89ca2;text-transform:uppercase;letter-spacing:1.5px'>Foto del paquete</p>" +
                "<img src='cid:paquete_district' alt='Paquete despachado' style='width:100%;max-width:100%;border:1px solid #E7E0DA;display:block' />" +
                "</div>" : "") +
            (string.IsNullOrEmpty(confirmarUrl) ? "" :
                "<div style='background:#F7F5FC;border:1px solid #E7E0DA;padding:20px;margin:0 0 4px;text-align:center'>" +
                "<p style='margin:0 0 4px;font-size:13px;color:#374151;font-weight:600'>¿Ya te llegó?</p>" +
                "<p style='margin:0 0 16px;font-size:12px;color:#a89ca2'>Confirma la entrega cuando lo tengas en tus manos.</p>" +
                "<a href='" + confirmarUrl + "' style='display:inline-block;" + Gradient + ";color:white;padding:12px 32px;text-decoration:none;font-size:14px;font-weight:600;letter-spacing:0.3px'>Confirmar recepción</a>" +
                "</div>");
        await SendAsync(to, "Tu pedido va en camino — District Store", Card(content),
            hasPhoto ? fotoBytes : null, fotoMimeType);
    }

    public async Task SendOrderApprovedAsync(string to, string nombre, int pedidoId, decimal total) =>
        await SendAsync(to, "¡Pedido aprobado! — District Store",
            Card(Header("Luz verde ✅") +
            "<p style='margin:0 0 16px;font-size:14px;color:#374151;line-height:1.6'>Hola <strong>" + nombre + "</strong>, tu pedido fue aprobado y lo despachamos en las próximas 72 horas.</p>" +
            "<div style='" + AccentBox + "'>" +
            "<p style='margin:0 0 4px;font-size:11px;color:#a89ca2;text-transform:uppercase;letter-spacing:1.5px'>Total</p>" +
            "<p style='margin:0;font-size:18px;font-weight:700;color:#241B22'>$" + total.ToString("N0") + "</p>" +
            "</div>" +
            "<p style='margin:0;font-size:13px;color:#a89ca2'>Te avisamos apenas salga de bodega con la info de envío.</p>"));

    public async Task SendCampaignEmailAsync(string to, string asunto, string mensaje, string unsubscribeUrl)
    {
        var mensajeHtml = System.Net.WebUtility.HtmlEncode(mensaje).Replace("\n", "<br/>");
        await SendAsync(to, asunto,
            Card(Header(asunto) +
            "<p style='margin:0 0 20px;font-size:14px;color:#374151;line-height:1.7'>" + mensajeHtml + "</p>" +
            "<p style='margin:0;font-size:11px;color:#b9aedd'><a href='" + unsubscribeUrl + "' style='color:#b9aedd'>Darme de baja de este correo</a></p>"));
    }
}
