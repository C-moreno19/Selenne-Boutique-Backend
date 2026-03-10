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
}