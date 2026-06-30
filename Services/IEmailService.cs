namespace SelenneApi.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string to, string nombre);
    Task SendVerificationEmailAsync(string to, string nombre, string token);
    Task SendPasswordResetEmailAsync(string to, string nombre, string token);
    Task SendPasswordChangedEmailAsync(string to, string nombre);
    Task SendNewUserCreatedEmailAsync(string to, string nombre, string tempPassword);
    Task SendOrderConfirmationClienteAsync(string to, string nombre, int pedidoId, decimal total);
    Task SendOrderConfirmationAdminAsync(string adminEmail, string clienteNombre, int pedidoId, decimal total);
    Task SendOrderStatusUpdateAsync(string to, string nombre, int pedidoId, string nuevoEstado, string? motivo = null);
    Task SendOrderApprovedAsync(string to, string nombre, int pedidoId, decimal total);
    Task SendPendingPaymentEmailAsync(string to, string nombre, int pedidoId, decimal total, string mensaje, string banco, string cuenta, string titular, string tipoCuenta);
    Task SendShippingNotificationEmailAsync(string to, string nombre, int pedidoId, string? numeroGuia, string? transportadora, byte[]? fotoBytes = null, string? fotoMimeType = null, string? confirmarUrl = null);
}
