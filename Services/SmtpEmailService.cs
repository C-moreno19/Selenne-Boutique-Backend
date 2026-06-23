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
        await SendAsync(to, "Pedido #" + pedidoId + " confirmado", Wrap("Pedido confirmado",
            "<p>Hola " + nombre + ", tu pedido #" + pedidoId + " fue recibido exitosamente.</p>" +
            "<p>Total: <b>$" + total.ToString("N2") + "</b></p>"));

    public async Task SendOrderConfirmationAdminAsync(string adminEmail, string clienteNombre, int pedidoId, decimal total) =>
        await SendAsync(adminEmail, "Nuevo pedido #" + pedidoId, Wrap("Nuevo pedido",
            "<p>Cliente: <b>" + clienteNombre + "</b></p>" +
            "<p>Total: <b>$" + total.ToString("N2") + "</b></p>"));

    public async Task SendOrderStatusUpdateAsync(string to, string nombre, int pedidoId, string nuevoEstado) =>
        await SendAsync(to, "Pedido #" + pedidoId + ": " + nuevoEstado, Wrap("Estado actualizado",
            "<p>Hola " + nombre + ", tu pedido #" + pedidoId + " ahora esta en estado: <b>" + nuevoEstado + "</b></p>"));

    public async Task SendPaymentInfoEmailAsync(string to, string nombre, int pedidoId, decimal total, string banco, string numeroCuenta, string titular, string tipoCuenta, string? mensajeAdicional)
    {
        var body = "<p>Hola <b>" + nombre + "</b>, para completar tu pedido <b>#" + pedidoId + "</b> por un total de <b>$" + total.ToString("N2") + "</b>, realiza la transferencia a los siguientes datos:</p>" +
            "<table style='border-collapse:collapse;margin:16px 0'>" +
            "<tr><td style='padding:6px 12px;font-weight:bold'>Banco</td><td style='padding:6px 12px'>" + banco + "</td></tr>" +
            "<tr style='background:#fef6fa'><td style='padding:6px 12px;font-weight:bold'>Número de cuenta</td><td style='padding:6px 12px'>" + numeroCuenta + "</td></tr>" +
            "<tr><td style='padding:6px 12px;font-weight:bold'>Titular</td><td style='padding:6px 12px'>" + titular + "</td></tr>" +
            "<tr style='background:#fef6fa'><td style='padding:6px 12px;font-weight:bold'>Tipo de cuenta</td><td style='padding:6px 12px'>" + tipoCuenta + "</td></tr>" +
            "</table>" +
            (string.IsNullOrEmpty(mensajeAdicional) ? "" : "<p><b>Mensaje:</b> " + mensajeAdicional + "</p>") +
            "<p>Una vez realizado el pago, envía el comprobante por WhatsApp o adjúntalo en la plataforma.</p>";
        await SendAsync(to, "Información de pago - Pedido #" + pedidoId, Wrap("Datos bancarios para tu pedido", body));
    }

    public async Task SendShippingEmailAsync(string to, string nombre, int pedidoId, string? numeroGuia, string? transportadora, string? fotoUrl)
    {
        var body = "<p>Hola <b>" + nombre + "</b>, tu pedido <b>#" + pedidoId + "</b> ha sido enviado.</p>" +
            "<table style='border-collapse:collapse;margin:16px 0'>" +
            (string.IsNullOrEmpty(transportadora) ? "" : "<tr><td style='padding:6px 12px;font-weight:bold'>Transportadora</td><td style='padding:6px 12px'>" + transportadora + "</td></tr>") +
            (string.IsNullOrEmpty(numeroGuia) ? "" : "<tr style='background:#fef6fa'><td style='padding:6px 12px;font-weight:bold'>Número de guía</td><td style='padding:6px 12px'><b>" + numeroGuia + "</b></td></tr>") +
            "</table>" +
            (string.IsNullOrEmpty(fotoUrl) ? "" : "<p><img src='" + fotoUrl + "' alt='Foto del paquete' style='max-width:400px;border-radius:8px;margin-top:8px'/></p>") +
            "<p>Puedes rastrear tu pedido con el número de guía en el sitio web de la transportadora.</p>";
        await SendAsync(to, "Tu pedido #" + pedidoId + " ha sido enviado", Wrap("¡Tu pedido está en camino!", body));
    }
}