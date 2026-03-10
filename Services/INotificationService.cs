namespace SelenneApi.Services;

public interface INotificationService
{
    Task CreateAsync(int usuarioId, string titulo, string mensaje, string tipo = "info", string? referencia = null);
    Task CreateBulkAsync(List<int> usuarioIds, string titulo, string mensaje, string tipo = "info");
    Task CreateForAllAsync(string titulo, string mensaje, string tipo = "info");
    Task CreateForRoleAsync(string rolNombre, string titulo, string mensaje, string tipo = "info");
    Task MarkAsReadAsync(int notificacionId, int usuarioId);
    Task MarkAllAsReadAsync(int usuarioId);
}
